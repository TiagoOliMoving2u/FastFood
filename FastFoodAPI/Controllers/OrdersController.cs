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
        public async Task<IActionResult> GetOrders()
        {
            var orders = await orderRepository.GetAllOrdersAsync();
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

        [HttpGet("FiltredByArea/{areaId:int}")]
        public async Task<IActionResult> GetOrdersByArea(int areaId, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByAreaAsync(areaId, cancellationToken);
            if (orders == null) return NotFound();
            return Ok(orders);
        }

        [HttpGet("FiltredByState/{state:int}")]
        public async Task<IActionResult> GetOrdersByState(int state, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByStateAsync(state, cancellationToken);
            if (orders == null) return NotFound();
            return Ok(orders);
        }

        [HttpGet("FiltredByStateAndArea/{state:int}/{areaId:int}")]
        public async Task<IActionResult> GetOrdersByStateAndArea(int state, int areaId, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByStateAndAreaAsync(state, areaId, cancellationToken);
            if (orders == null) return NotFound();
            return Ok(orders);
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
