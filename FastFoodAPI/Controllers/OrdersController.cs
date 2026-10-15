using FastFoodAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using FastFoodAPI.Repositories;

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

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await orderRepository.GetAllOrdersAsync();

            return Ok(orders);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetNameById(int id, CancellationToken cancellationToken)
        {
            var order = await orderRepository.GetByIdAsync(id, cancellationToken);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);

        }
    }
}
