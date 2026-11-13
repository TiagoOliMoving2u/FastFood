using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.Models;
using FastFoodAPI.DTOs;
using System.Text;

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
            
            return await connection.QuerySingleOrDefaultAsync<User>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        NormalizedUsername = normalizedUsername
                    },
                    cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string sql = """
            SELECT
                u.Id,
                u.Username,
                u.RoleId,
                r.Name AS RoleName,
                u.IsActive
            FROM dbo.Users u
            INNER JOIN dbo.Roles r
                ON u.RoleId = r.Id;
            """;
            var users = await connection.QueryAsync<UserResponse>(
                new CommandDefinition(
                    sql,
                    cancellationToken: cancellationToken));
            return users.AsList();
        }

        public async Task<bool> UpdateUserDetailsAsync(
            UpdateUserDetailsRequest request,
            CancellationToken cancellationToken = default)
        {
            var fields = new List<string>();
            var parameters = new DynamicParameters();

            parameters.Add("Id", request.UserId);

            if (request.RoleId != null)
            {
                fields.Add("RoleId = @RoleId");
                parameters.Add("RoleId", request.RoleId);
            }

            if (request.IsAvailable != null)
            {
                fields.Add("IsActive = @IsAvailable");
                parameters.Add("IsAvailable", request.IsAvailable);
            }

            if (fields.Count == 0)
                return false;

            var sql = $"""
                UPDATE dbo.Users
                SET {string.Join(", ", fields)}
                WHERE Id = @Id;
                """;

            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: cancellationToken));

            return affectedRows > 0;
        }
    }
}
