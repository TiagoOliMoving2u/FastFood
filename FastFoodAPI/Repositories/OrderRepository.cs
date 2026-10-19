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

        // -----GETTERS-----
        //method to get all orders with their area names
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

        //method to get an order by its ID
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

            if (order is not null)
            {
                order.CreatedOn = DateTime.SpecifyKind(order.CreatedOn, DateTimeKind.Utc);
            }

            return order;
        }

        // Method to get all orders filtered by areaId
        public async Task<IReadOnlyList<OrderResponse?>> GetOrdersByAreaAsync(int areaId, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sqlGetOrdersByArea = """
                SELECT o.Id, o.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                From dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                WHERE o.AreaId = @AreaId;
                """;

            var orders = await connection.QueryAsync<OrderResponse>(new CommandDefinition(
                sqlGetOrdersByArea,
                new { AreaId = areaId },
                cancellationToken: cancellationToken
                ));

            return orders.AsList();
        }

        // Method to get all orders filtered by state
        public async Task<IReadOnlyList<OrderResponse?>> GetOrdersByStateAsync(int state, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sqlGetOrdersByState = """
                SELECT o.Id, o.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                From dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                WHERE o.State = @State;
                """;

            var orders = await connection.QueryAsync<OrderResponse>(new CommandDefinition(
                sqlGetOrdersByState,
                new { State = state },
                cancellationToken: cancellationToken
                ));

            return orders.AsList();
        }

        // method to get all orders filtered by state and areaId
        public async Task<IReadOnlyList<OrderResponse?>> GetOrdersByStateAndAreaAsync(int state, int areaId, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            const string sqlGetOrdersByState = """
                SELECT o.Id, o.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                From dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                WHERE o.State = @State AND o.AreaId = @AreaId;
                """;

            var orders = await connection.QueryAsync<OrderResponse>(new CommandDefinition(
                sqlGetOrdersByState,
                new { State = state, AreaId = areaId },
                cancellationToken: cancellationToken
                ));

            return orders.AsList();
        }

        // -----POSTS-----
        // Method to create a new order with a unique code based on the area acronym and the last order number
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

    }
}
