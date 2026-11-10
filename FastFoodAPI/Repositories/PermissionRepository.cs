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

        public async Task<IReadOnlyList<PermissionResponse>>
        GetPermissionsAsync(
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
    }
}
