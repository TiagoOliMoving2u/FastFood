using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using System.Data;
using System.Text;


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

        // method to get orders filtered by areaId and/or state
        public async Task<IReadOnlyList<OrderResponse?>> GetOrdersFiltredAsync(
            int? areaId = null,
            int? state = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var sqlGetOrdersFiltred = new StringBuilder("""
                SELECT o.Id, o.Code, o.AreaId, a.Name AS AreaName, o.State, o.CreatedOn
                From dbo.Orders o
                INNER JOIN dbo.Areas a ON o.AreaId = a.Id
                WHERE 1 = 1
                """);

            var parameters = new DynamicParameters();

            if (areaId.HasValue)
            {
                sqlGetOrdersFiltred.AppendLine(" AND o.AreaId = @AreaId");
                parameters.Add("@AreaId", areaId.Value);
            }

            if (state.HasValue)
            {
                sqlGetOrdersFiltred.AppendLine(" AND o.State = @State");
                parameters.Add("@State", state.Value);
            }

            sqlGetOrdersFiltred.AppendLine(" ORDER BY o.Id;");

            var orders = await connection.QueryAsync<OrderResponse>(new CommandDefinition(
                sqlGetOrdersFiltred.ToString(),
                parameters,
                cancellationToken: cancellationToken
            ));

            if (orders is null)
                return orders.AsList();

            const string sqlGetOrderItems = """
            SELECT
                oi.Id,
                oi.OrderId,
                oi.MenuItemId,
                mi.Name,
                oi.Quantity,
                oi.UnitPrice
            FROM dbo.OrderItems oi
            INNER JOIN dbo.MenuItems mi ON mi.Id = oi.MenuItemId
            WHERE oi.OrderId IN @OrderIds;
            """;

            var orderIds = orders.Select(o => o.Id).ToArray();

            var orderItems = await connection.QueryAsync<OrderItem>(
                new CommandDefinition(
                    sqlGetOrderItems,
                    new { OrderIds = orderIds },
                    cancellationToken: cancellationToken
                )
            );

            var itemsByOrder = orderItems.ToLookup(i => i.OrderId);

            foreach (var order in orders)
            {
                order.Items = itemsByOrder[order.Id].ToList();
            }

            return orders.AsList();

        }
        //getters history state
        //method to get the history of state changes for a specific order
        public async Task<IReadOnlyList<StateHistoryResponse>> GetHistoryStateFiltredAsync(
            int? orderId = null,
            int? areaId = null,
            int? oldState = null,
            int? newState = null,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            var sqlString = new StringBuilder("""
                SELECT h.Id, h.OrderId,o.Code AS OrderCode, o.AreaId, a.Name AS AreaName, h.OldState, h.NewState, h.ChangedOn
                FROM dbo.StateHistory h
                INNER JOIN dbo.Orders o
                    ON h.OrderId = o.Id
                INNER JOIN dbo.Areas a
                    ON o.AreaId = a.Id
                WHERE 1 = 1
                """);

            var parameters = new DynamicParameters();

            if (orderId.HasValue)
            {
                sqlString.AppendLine(" AND OrderId = @OrderId");
                parameters.Add("@OrderId", orderId.Value);
            }

            if (areaId.HasValue)
            {
                sqlString.AppendLine(" AND AreaId = @AreaId");
                parameters.Add("@AreaId", areaId.Value);
            }

            if (oldState.HasValue)
            {
                sqlString.AppendLine(" AND OldState = @OldState");
                parameters.Add("@OldState", oldState.Value);
            }

            if (newState.HasValue)
            {
                sqlString.AppendLine(" AND NewState = @NewState");
                parameters.Add("@NewState", newState.Value);
            }

            sqlString.AppendLine(" ORDER BY Id;");

            var history = await connection.QueryAsync<StateHistoryResponse>(new CommandDefinition(
                sqlString.ToString(),
                parameters,
                cancellationToken: cancellationToken
                ));

            return history.AsList();

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

            var area = await connection.QuerySingleOrDefaultAsync<Area>(new CommandDefinition(
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

            var sqlDecrementStock = """
                UPDATE dbo.MenuItems
                SET AvailableUnits = AvailableUnits - @Quantity
                WHERE Id = @MenuItemId AND AvailableUnits >= @Quantity;
                """;

            foreach (var item in request.Items)
            {
                var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(
                    sqlDecrementStock,
                    new
                    {
                        MenuItemId = item.Id,
                        Quantity = item.Quantity
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                ));

                if (affectedRows != 1)
                {
                    throw new ArgumentException($"MenuItem with Id { item.Id} does not have enough available units to fulfill the order.");
                }
            }

                var sqlInsertOrderItems = """
                INSERT INTO dbo.OrderItems (OrderId, MenuItemId, Quantity, UnitPrice)
                VALUES (@OrderId, @MenuItemId, @Quantity, @UnitPrice);
                """;

            var parameters = request.Items.Select(item => new
            {
                OrderId = order.Id,
                MenuItemId = item.Id,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });

            var itemsAffectedRows = await connection.ExecuteAsync(new CommandDefinition(
                sqlInsertOrderItems,
                parameters,
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            var sqlInsertOrderHistory = """
                INSERT INTO dbo.StateHistory (OrderId, NewState)
                VALUES (@OrderId, @NewState);
                """;

            var historyAffectedRows = await connection.ExecuteAsync(new CommandDefinition(
                sqlInsertOrderHistory,
                new
                {
                    OrderId = order.Id,
                    NewState = order.State,
                },
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            // 4. Confirmar a transação
            await transaction.CommitAsync(cancellationToken);

            return order;

        }

        // -----UPDATES-----
        // Method to update the state of an order
        public async Task<OrderResponse?> UpdateOrderStateAsync(int orderId,OrderState oldState,OrderState newState, CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            const string sqlUpdateOrderState = """
                UPDATE dbo.Orders
                SET State = @NewState
                WHERE Id = @OrderId;
                """;

            var affectedRows = await connection.ExecuteAsync(new CommandDefinition(
                sqlUpdateOrderState,
                new { NewState = newState, OrderId = orderId },
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            if (affectedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            var sqlInsertOrderHistory = """
                INSERT INTO dbo.StateHistory (OrderId, OldState, NewState)
                VALUES (@OrderId, @OldState, @NewState);
                """;

            var historyAffectedRows = await connection.ExecuteAsync(new CommandDefinition(
                sqlInsertOrderHistory,
                new
                {
                    OrderId = orderId,
                    OldState = oldState,
                    NewState = newState,
                },
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            await transaction.CommitAsync(cancellationToken);

            var order = await GetByIdAsync(orderId, cancellationToken);

            return order;
        }

        

        public async Task ResetAllAsync(
        CancellationToken cancellationToken = default)
        {
            await using var connection = _factory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            
            await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            const string sqlResetOrderItems = """
                DELETE FROM dbo.OrderItems;
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                sqlResetOrderItems,
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            const string sqlResetStateHistory = """
                DELETE FROM dbo.StateHistory;
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                sqlResetStateHistory,
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            const string sqlResetOrders = """
                DELETE FROM dbo.Orders;
                """;

            var affectedRowsOrders = await connection.ExecuteAsync(new CommandDefinition(
                sqlResetOrders,
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            const string sqlResetAreas = """
                UPDATE dbo.Areas
                SET LastOrderNumber = 0;
                """;

            await connection.ExecuteAsync(new CommandDefinition(
                sqlResetAreas,
                transaction: transaction,
                cancellationToken: cancellationToken
            ));

            await transaction.CommitAsync(cancellationToken);

        }
    }
}


