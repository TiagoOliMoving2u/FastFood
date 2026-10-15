using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using System.Data;

namespace FastFoodAPI.Repositories
{
    public class OrderRepository
    {
        private readonly SqlConnectionFactory _factory;

        public OrderRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<OrderResponse?> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted,
                cancellationToken);

            // Increment the area counter (LastOrderNumber) 
            const string sqlIncrementCounterArea = """
                UPDATE dbo.Areas
                SET LastOrderNumber = LastOrderNumber + 1
                OUTPUT
                INSERTED.Id,
                    INSERTED.Name,
                    INSERTED.Acronym,
                    INSERTED.LastOrderNumber
                WHERE Id = @AreaId;
                """;

            var area = await connection.QuerySingleAsync<Area>(new CommandDefinition(
                    sqlIncrementCounterArea,
                    new { AreaId = request.AreaId },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                ));

            if (area is null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Generate the order code based on the area acronym and the last order number
            var orderCode = $"{area.Acronym}-{area.LastOrderNumber:D4}";

            // Insert the new order into the Orders table
            const string sqlInsertOrder = """
                INSERT INTO dbo.Orders (Code, AreaId, State)
                OUTPUT 
                    INSERTED.Id,
                    INSERTED.Code,
                    INSERTED.AreaId,
                    INSERTED.State,
                    INSERTED.CreatedOn
                VALUES
                    (@Code, @AreaId, @State)
                """;

            var order = await connection.QuerySingleAsync<OrderResponse>(
                new CommandDefinition(
                    sqlInsertOrder,
                    new
                    {
                        Code = orderCode,
                        AreaId = request.AreaId,
                        State = OrderState.AlredyOrdered
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                ));
            
            order.AreaName = area.Name;
            order.CreatedOn = DateTime.SpecifyKind(
                order.CreatedOn,
                DateTimeKind.Utc);

            // 4. Confirmar a transação
            await transaction.CommitAsync(cancellationToken);

            return order;

        }

        public async Task<IReadOnlyList<OrderResponse>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            const string query = """
                SELECT o.Id, o.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                FROM dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                ORDER BY o.Id;
                """;
            
            var orders = await connection.QueryAsync<OrderResponse>(new CommandDefinition(query, cancellationToken: cancellationToken));
            return orders.AsList();
        }

        public async Task<OrderResponse?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sqlGetOrderById = """
                SELECT o.Id, O.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                FROM dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                Where o.Id = @OrderId;
                """;

            var order = await connection.QuerySingleOrDefaultAsync<OrderResponse>(new CommandDefinition(
                sqlGetOrderById,
                new { OrderId = orderId },

                cancellationToken: cancellationToken
                ));

            if(order is not null)
            {
                order.CreatedOn = DateTime.SpecifyKind(order.CreatedOn, DateTimeKind.Utc);
            }

            return order;
        }

    }
}
