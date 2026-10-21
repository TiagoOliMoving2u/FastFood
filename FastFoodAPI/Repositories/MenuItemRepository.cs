using FastFoodAPI.Data;
using Dapper;
using FastFoodAPI.DTOs;
namespace FastFoodAPI.Repositories
{
    public class MenuItemRepository
    {
        private readonly SqlConnectionFactory _factory;

        public MenuItemRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<MenuItemResponse>> GetMenuItemsAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string query = """
                SELECT
                    m.Id,
                    m.Name,
                    m.Description,
                    m.Price,
                    c.Name AS Category,
                    m.AvailableUnits
                FROM dbo.MenuItems m
                INNER JOIN dbo.Categories c
                    ON m.CategoryId = c.Id;
                """;

            var items = await connection.QueryAsync<MenuItemResponse>(new CommandDefinition(
                query, 
                cancellationToken: cancellationToken));

            return items.AsList();
        }
    }
}
