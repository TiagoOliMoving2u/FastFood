using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderRepository orderRepository;
        public OrdersController(OrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        //--- Get: api/orders

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? areaId = null,
            [FromQuery] int? state = null,
            CancellationToken cancellationToken = default)
        {
            var orders = await orderRepository.GetOrdersFiltredAsync(areaId, state, cancellationToken);
            if (orders == null) return NotFound();
            return Ok(orders);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetNameById(int id, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);
            if (order == null) return NotFound();
            return Ok(order);

        }

        [HttpGet("StateHistory")]
        public async Task<IActionResult> GetStateHistory(
            [FromQuery] int? orderId = null,
            [FromQuery] int? areaId = null,
            [FromQuery] int? oldState = null,
            [FromQuery] int? newState = null,
            CancellationToken cancellationToken = default)
        {
            if (orderId is <= 0 || areaId is <= 0)
                return BadRequest("Invalid order or area ID.");

            if (oldState is < 1 or > 4 ||
                newState is < 1 or > 4)
                return BadRequest("Invalid order state.");

            var history = await orderRepository.GetHistoryStateFiltredAsync(orderId, areaId, oldState, newState, cancellationToken);
            if (history == null) return NotFound();
            return Ok(history);
        }

        //--- Post: api/orders

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            if( request.AreaId <= 0)
            {
                return BadRequest("Invalid AreaId.");
            }

            var order = await orderRepository.CreateAsync(request, cancellationToken);

            if (order is null)
            {
                return BadRequest("A área indicada não existe.");
            }

            return CreatedAtAction(nameof(GetNameById),
                new { id = order.Id },
                order);
        }

        //--- Put: api/orders/{id}


        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderResponse>> UpdateOrder(int id, CancellationToken cancellationToken)
        {
            if( id <= 0 )
            {
                return BadRequest("Invalid order ID.");
            }

            var order = await orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
            {
                return NotFound();
            }

            var nextState = (OrderState)((int)order.State + 1);

            if (!Enum.IsDefined(nextState))
            {
                return BadRequest("Order has no next state.");
            }

            var updatedOrder = await orderRepository.UpdateOrderStateAsync(id, order.State, nextState, cancellationToken);

            if (updatedOrder is null)
            {
                return NotFound();
            }

            return Ok(updatedOrder);
        }

    }
}
