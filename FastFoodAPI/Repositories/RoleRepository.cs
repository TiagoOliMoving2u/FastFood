using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using System.Text;

namespace FastFoodAPI.Repositories
{
    public class RoleRepository
    {

        private SqlConnectionFactory _factory;

        public RoleRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<Role>?> GetRolesAsync(
            bool? inactiveToo,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            var sql = new StringBuilder( """
            SELECT
                r.Id,
                r.Name,
                r.Description,
                r.IsActive
            FROM dbo.Roles r
            WHERE 1 = 1
            """);


            if (inactiveToo == false)
            {
                sql.AppendLine(" AND r.IsActive = 1");
            }

            sql.AppendLine(" ORDER BY r.Id;");

            return (await connection.QueryAsync<Role>(
                new CommandDefinition(
                    sql.ToString(),
                    cancellationToken: cancellationToken))).ToList();
        }

        /*
        public async Task<bool> CreateRoleAsync(
            CreateRoleRequest createRoleRequest,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string sql = """
            INSERT INTO dbo.Roles
                (Name, Description, IsActive)
            SELECT
                @Name, @Description, 1
            WHERE NOT EXISTS (
                SELECT 1
                FROM dbo.Roles
                WHERE Name = @Name
            );
            """;

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        createRoleRequest.Name,
                        createRoleRequest.Description
                    },
                    cancellationToken: cancellationToken));

            return rowsAffected > 0;
        } */


        public async Task<bool> CreateRoleAsync(
            CreateRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            var permissionIds = (request.PermissionIds ?? new())
                .Distinct()
                .ToArray();

            // 1. Verificar se todas as permissões existem
            if (permissionIds.Length > 0)
            {
                const string sqlCount = """
                    SELECT COUNT(*)
                    FROM dbo.Permissions
                    WHERE Id IN @PermissionIds;
                    """;

                var count = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        sqlCount,
                        new { PermissionIds = permissionIds },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                if (count != permissionIds.Length)
                    throw new ArgumentException(
                        "Uma ou mais permissões não existem.");
            }

            // 2. Criar a Role e obter o ID gerado
            const string sqlCreateRole = """
                INSERT INTO dbo.Roles
                    (Name, Description, IsActive)
                OUTPUT INSERTED.Id
                SELECT @Name, @Description, 1
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM dbo.Roles WITH (UPDLOCK, HOLDLOCK)
                    WHERE Name = @Name
                );
                """;

            var roleId = await connection.QuerySingleOrDefaultAsync<int?>(
                new CommandDefinition(
                    sqlCreateRole,
                    new
                    {
                        request.Name,
                        request.Description
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // A Role já existe
            if (roleId is null)
                return false;

            // 3. Associar as permissões à Role
            if (permissionIds.Length > 0)
            {
                const string sqlRolePermissions = """
                    INSERT INTO dbo.RolePermissions
                        (RoleId, PermissionId)
                    SELECT
                        @RoleId, Id
                    FROM dbo.Permissions
                    WHERE Id IN @PermissionIds;
                    """;

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        sqlRolePermissions,
                        new
                        {
                            RoleId = roleId.Value,
                            PermissionIds = permissionIds
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));
            }

            // 4. Confirmar todas as alterações
            await transaction.CommitAsync(cancellationToken);

            return true;
        }

        public async Task<IReadOnlyList<int>?> GetRolePermissionIdsAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sqlRole = """
                SELECT Id
                FROM dbo.Roles
                WHERE Id = @RoleId;
                """;

            var existingRole = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    sqlRole,
                    new { RoleId = roleId },
                    cancellationToken: cancellationToken));

            if (existingRole is null)
                return null;

            const string sql = """
                SELECT PermissionId
                FROM dbo.RolePermissions
                WHERE RoleId = @RoleId
                ORDER BY PermissionId;
                """;

            var ids = await connection.QueryAsync<int>(
                new CommandDefinition(
                    sql,
                    new { RoleId = roleId },
                    cancellationToken: cancellationToken));

            return ids.AsList();
        }

        public async Task<bool> UpdateRolePermissionsAsync(
            int roleId,
            IReadOnlyCollection<int> permissionIds,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            // 1. Verificar se a Role existe
            const string sqlRole = """
                SELECT Id
                FROM dbo.Roles WITH (UPDLOCK, HOLDLOCK)
                WHERE Id = @RoleId;
                """;

            var existingRole = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    sqlRole,
                    new { RoleId = roleId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (existingRole is null)
                return false;

            // 2. Remover IDs duplicados
            var ids = permissionIds.Distinct().ToArray();

            // 3. Validar se todas as permissões existem
            if (ids.Length > 0)
            {
                const string sqlCount = """
                    SELECT COUNT(*)
                    FROM dbo.Permissions
                    WHERE Id IN @PermissionIds;
                    """;

                var count = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        sqlCount,
                        new { PermissionIds = ids },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                if (count != ids.Length)
                {
                    throw new ArgumentException(
                        "Uma ou mais permissões não existem.");
                }
            }

            // 4. Remover as associações anteriores
            const string sqlDelete = """
                DELETE FROM dbo.RolePermissions
                WHERE RoleId = @RoleId;
                """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sqlDelete,
                    new { RoleId = roleId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 5. Inserir as novas associações
            if (ids.Length > 0)
            {
                const string sqlInsert = """
                    INSERT INTO dbo.RolePermissions
                        (RoleId, PermissionId)
                    SELECT @RoleId, Id
                    FROM dbo.Permissions
                    WHERE Id IN @PermissionIds;
                    """;

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        sqlInsert,
                        new
                        {
                            RoleId = roleId,
                            PermissionIds = ids
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));
            }

            // 6. Confirmar todas as alterações
            await transaction.CommitAsync(cancellationToken);

            return true;
        }

        public async Task<bool> UpdateRoleStateAsync(
            UpdateRoleStateRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string sql = """
                UPDATE dbo.Roles
                SET IsActive = @IsActive
                WHERE Id = @RoleId AND IsActive != @IsActive;
                """;
            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        RoleId = request.Id,
                        IsActive = request.IsActive
                    },
                    cancellationToken: cancellationToken));
            return rowsAffected > 0;
        }
    }
}
