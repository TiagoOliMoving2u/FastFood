using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.DTOs;
using System.Data.SqlTypes;
using System.Text;
namespace FastFoodAPI.Repositories
{
    public class MenuItemRepository
    {
        private readonly SqlConnectionFactory _factory;

        public MenuItemRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<MenuItemResponse>> GetMenuItemsAsync(
            int? id,
            int[]? ids,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var query = new StringBuilder("""
                SELECT
                    m.Id,
                    m.Name,
                    m.Description,
                    m.Price,
                    c.Name AS Category,
                    m.AvailableUnits
                FROM dbo.MenuItems m
                INNER JOIN dbo.Categories c
                    ON m.CategoryId = c.Id
                WHERE 1=1
                """);

            var parameters = new DynamicParameters();

            if (id.HasValue)
            {
                query.AppendLine(" AND m.Id = @Id");
                parameters.Add("@Id", id.Value);
            }
            else if (ids != null && ids.Length > 0)
            {
                query.AppendLine(" AND m.Id IN @Ids");
                parameters.Add("@Ids", ids);
            }

            query.AppendLine(" ORDER BY Id;");

            var items = await connection.QueryAsync<MenuItemResponse>(new CommandDefinition(
                query.ToString(),
                parameters,
                cancellationToken: cancellationToken));

            return items.AsList();
        }
    }
}
