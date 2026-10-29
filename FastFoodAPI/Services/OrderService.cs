using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Mvc;
using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace FastFoodAPI.Services
{
    public class OrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly MenuItemRepository _menuItemRepository;

        public OrderService(
            OrderRepository orderRepository,
            MenuItemRepository menuItemRepository)
        {
            _orderRepository = orderRepository;
            _menuItemRepository = menuItemRepository;
        }

        //Getters

        public async Task<IReadOnlyList<OrderResponse?>> GetOrdersAsyn(
            int? areaId,
            int? state,
            CancellationToken cancellationToken = default)
        {
            if (areaId is <= 0)
                throw new ArgumentException("Invalid area ID.");

            if (state is < 1 or > 4)
                throw new ArgumentException("Invalid order state.");

            var orders = await _orderRepository.GetOrdersFiltredAsync(areaId, state, cancellationToken);

            
            return orders;
        }

        public async Task<ActionResult<OrderResponse?>> GetOrderByIdAsync(int id, CancellationToken cancellationToken)
        {   
            if(id <= 0)
                return new BadRequestObjectResult("Invalid order ID.");

            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
            return order;
        }


        public async Task<IReadOnlyList<StateHistoryResponse?>> GetStateHistoryAsync(
            int? orderId,
            int? areaId,
            int? oldState,
            int? newState,
            CancellationToken cancellationToken = default)
        {
            if (orderId is <= 0 || areaId is <= 0)
                throw new ArgumentException("Invalid order or area ID.");

            if (oldState is < 1 or > 4 ||
                newState is < 1 or > 4)
                throw new ArgumentException("Invalid order state.");

            var history = await _orderRepository.GetHistoryStateFiltredAsync(orderId, areaId, oldState, newState, cancellationToken);
            
            return history;
        }

        //POSTs

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            if (request.AreaId <= 0)
            {
                throw new ArgumentException("Invalid AreaId.");
            }

            if (request.Items is null || request.Items.Count == 0)
            {
                throw new ArgumentException("Order must contain at least one item.");
            }

            if (request.Items.Any(item => item.Quantity <= 0))
            {
                throw new ArgumentException("All items must have a quantity greater than zero.");
            }

            if (request.Items.Any(item => item.Id <= 0))
            {
                throw new ArgumentException("All items must have a valid MenuItemId.");
            }

            foreach (var item in request.Items)
            {
                foreach (var otherItem in request.Items.Where(i => i != item && i.Id == item.Id && i.Quantity > 0))
                {
                    item.Quantity += otherItem.Quantity;
                    otherItem.Quantity = 0; // Mark the duplicate item for removal
                }
            }

            foreach (var item in request.Items.Where(i => i.Quantity == 0).ToList())
            {
                request.Items.Remove(item); // Remove the duplicate items with zero quantity
            }
            
            var menuItemIds = request.Items.Select(i => i.Id).ToList();

            var menuItems = await _menuItemRepository.GetMenuItemsAsync(null, menuItemIds?.ToArray(), cancellationToken);

            if(menuItems.Count != menuItemIds.Count)
            {
                throw new ArgumentException("One or more MenuItemIds are invalid.");
            }

            foreach (var item in request.Items)
            {
                var menuItem = menuItems.FirstOrDefault(mi => mi.Id == item.Id);
                if (menuItem is null)
                    throw new ArgumentException($"MenuItem with ID {item.Id} does not exist.");
                else if (item.Quantity > menuItem.AvailableUnits)
                    throw new ArgumentException($"No enought units from {menuItem.Name}.");

                item.Price = menuItem.Price;
            }

            var order = await _orderRepository.CreateAsync(request, cancellationToken);
            //TODO: verificar
            if (order is null)
            {
                throw new ArgumentException("A área indicada não existe.");
            }

            return order;
        }

        //PUTs (atualziar)

        public async Task<OrderResponse> UpdateOrderAsync(int id, int operation,CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid order ID.");
            }

            if (operation != -1 && operation != 1)
                throw new ArgumentException("State just can increment or decrement 1 unit each time.");

            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
            {
                throw new ArgumentException("Order not found.");
            }

            var nextState = (OrderState)((int)order.State + operation);

            if (!Enum.IsDefined(nextState))
            {
                throw new ArgumentException("Order has no next state.");
            }

            var updatedOrder = await _orderRepository.UpdateOrderStateAsync(id, order.State, nextState, cancellationToken);

            if (updatedOrder is null)
            {
                throw new ArgumentException("Failed to update order state.");
            }

            return updatedOrder;
        }

        
    }
}
