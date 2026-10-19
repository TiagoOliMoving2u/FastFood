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

        //--- Get: api/orders

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

        [HttpGet("FiltredByArea/{areaId:int}")]
        public async Task<IActionResult> GetOrdersByArea(int areaId, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByAreaAsync(areaId, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("FiltredByState/{state:int}")]
        public async Task<IActionResult> GetOrdersByState(int state, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByStateAsync(state, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("FiltredByStateAndArea/{state:int}/{areaId:int}")]
        public async Task<IActionResult> GetOrdersByStateAndArea(int state, int areaId, CancellationToken cancellationToken)
        {
            var orders = await orderRepository.GetOrdersByStateAndAreaAsync(state, areaId, cancellationToken);
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

    }
}
