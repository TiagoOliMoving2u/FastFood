using Dapper;
using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Data;

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
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string sql = """
            SELECT
                r.Id,
                r.Name,
                r.Description,
                r.IsActive
            FROM dbo.Roles r
            WHERE r.IsActive = 1;
            """;
            return (await connection.QueryAsync<Role>(
                new CommandDefinition(
                    sql,
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

    }
}
