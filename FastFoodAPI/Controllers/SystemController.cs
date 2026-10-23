using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        public SystemController(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetAllAsync(CancellationToken cancellationToken = default)
        {
            await _orderRepository.ResetAllAsync(cancellationToken);
            return NoContent();
        }
    }
}
