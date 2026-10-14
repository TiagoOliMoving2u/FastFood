using FastFoodAPI.Data;
using Dapper;
using FastFoodAPI.DTOs;

namespace FastFoodAPI.Repositories
{
    public class AreaRepository
    {
        private readonly SqlConnectionFactory _factory;

        public AreaRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }
    
        public async Task<IReadOnlyList<AreaResponse>> GetAllAreas(CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            const string query = """
                SELECT Id, Name, Acronym
                FROM dbo.Areas
                ORDER BY Id;
                """;

            var command = new CommandDefinition(query, cancellationToken: cancellationToken);

            var areas = await connection.QueryAsync<AreaResponse>(command);

            return areas.AsList();
        }
    }
}
