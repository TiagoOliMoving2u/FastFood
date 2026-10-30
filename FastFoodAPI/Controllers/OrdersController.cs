using FastFoodAPI.DTOs;
using FastFoodAPI.Hubs;
using FastFoodAPI.Models;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly IHubContext<OrdersHub> _hubContext;
        public OrdersController(OrderService orderService,
            IHubContext<OrdersHub> hubContext)
        {
            _orderService = orderService;
            _hubContext = hubContext;
        }

        //--- Get: api/orders

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? areaId = null,
            [FromQuery] int? state = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var orders = await _orderService.GetOrdersAsyn(areaId, state, cancellationToken);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetOrderById(int id, CancellationToken cancellationToken)
        {
            try { 
                var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("StateHistory")]
        public async Task<IActionResult> GetStateHistory(
            [FromQuery] int? orderId = null,
            [FromQuery] int? areaId = null,
            [FromQuery] int? oldState = null,
            [FromQuery] int? newState = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var history = await _orderService.GetStateHistoryAsync(orderId, areaId, oldState, newState, cancellationToken);
                if (history == null) return NotFound();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        //--- Post: api/orders

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request, cancellationToken);

                if (_hubContext == null)
                {
                    return Conflict("Order state could not be created.");
                }
                    await _hubContext.Clients.All.SendAsync(
                        "OrdersChanged",
                        cancellationToken);

                    await _hubContext.Clients.All.SendAsync(
                        "CreatedNewOrder",
                        cancellationToken);

                return CreatedAtAction(nameof(GetOrderById),
                    new { id = order.Id },
                    order);
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex.Message });
            }
        }

        //--- Put: api/orders/{id}

        [Authorize(Policy = "CanUpdateOrders")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderResponse>> UpdateOrder(int id, [FromQuery] int operation,CancellationToken cancellationToken)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, operation,cancellationToken);

                if (updatedOrder is null)
                {
                    return Conflict("Order state could not be updated.");
                }

                await _hubContext.Clients.All.SendAsync(
                        "OrdersChanged",
                        cancellationToken);
                

                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        } 

    }
}
