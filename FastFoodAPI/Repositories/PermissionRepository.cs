using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.DTOs;

namespace FastFoodAPI.Repositories
{
    public class PermissionRepository
    {
        private readonly SqlConnectionFactory _factory;

        public PermissionRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<PermissionResponse>> GetPermissionsAsync(
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
            SELECT Id, Code, Description
            FROM dbo.Permissions
            ORDER BY Code;
            """;

            var permissions =
                await connection.QueryAsync<PermissionResponse>(
                    new CommandDefinition(
                        sql,
                        cancellationToken: cancellationToken));

            return permissions.AsList();
        }

        public async Task<bool> HasPermissionAsync(
            int userId,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Users u
                    INNER JOIN dbo.Roles r
                        ON u.RoleId = r.Id
                    INNER JOIN dbo.RolePermissions rp
                        ON rp.RoleId = r.Id
                    INNER JOIN dbo.Permissions p
                        ON rp.PermissionId = p.Id
                    WHERE u.Id = @UserId
                        AND u.IsActive = 1
                        AND r.IsActive = 1
                        AND p.Code = @PermissionCode
                )
                THEN 1 ELSE 0 END;
                """;

            var result = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        UserId = userId,
                        PermissionCode = permissionCode
                    },
                    cancellationToken: cancellationToken));

            return result == 1;
        }

        public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT DISTINCT p.Code
                FROM dbo.Users u
                INNER JOIN dbo.Roles r
                    ON u.RoleId = r.Id
                INNER JOIN dbo.RolePermissions rp
                    ON r.Id = rp.RoleId
                INNER JOIN dbo.Permissions p
                    ON rp.PermissionId = p.Id
                WHERE u.Id = @UserId
                    AND u.IsActive = 1
                    AND r.IsActive = 1
                ORDER BY p.Code;
                """;

            var permissions = await connection.QueryAsync<string>(
                new CommandDefinition(
                    sql,
                    new { UserId = userId },
                    cancellationToken: cancellationToken));

            return permissions.AsList();
        }
    }
}
