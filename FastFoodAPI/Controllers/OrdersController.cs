using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
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


        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderResponse>> UpdateOrder(int id, CancellationToken cancellationToken)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, cancellationToken);

                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
