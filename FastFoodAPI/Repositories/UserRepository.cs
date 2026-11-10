using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.Models;
using FastFoodAPI.DTOs;

namespace FastFoodAPI.Repositories
{
    public class UserRepository
    {
        private readonly SqlConnectionFactory _factory;

        public UserRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int?> CreateAsync(
        User user,
        string roleName,
        CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            const string sql = """
            INSERT INTO dbo.Users
                (Username, NormalizedUsername, PasswordHash, RoleId)
            OUTPUT INSERTED.Id
            SELECT
                @Username,
                @NormalizedUsername,
                @PasswordHash,
                r.Id
            FROM dbo.Roles r
            WHERE r.Name = @RoleName
              AND r.IsActive = 1;
            """;

            return await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        user.Username,
                        NormalizedUsername =
                            user.Username.ToUpperInvariant(),
                        user.PasswordHash,
                        RoleName = roleName
                    },
                    cancellationToken: cancellationToken));
        }

        public async Task<User?> GetByUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            const string sql = """
            SELECT
                u.Id,
                u.Username,
                u.PasswordHash,
                u.RoleId,
                u.IsActive,
                r.Name AS Role
            FROM dbo.Users u
            INNER JOIN dbo.Roles r
                ON u.RoleId = r.Id
            WHERE u.NormalizedUsername = @NormalizedUsername
                AND u.IsActive = 1
                AND r.IsActive = 1;
            """;

            var normalizedUsername =
                username.Trim().ToUpperInvariant();
            Console.WriteLine($"Normalized Username: {normalizedUsername}");
            return await connection.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        NormalizedUsername = normalizedUsername
                    },
                    cancellationToken: cancellationToken));
        }

        
    }
}
