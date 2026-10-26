using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize(Policy = "CanResetSystem")]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetAllAsync(CancellationToken cancellationToken = default)
        {
            await _orderRepository.ResetAllAsync(cancellationToken);
            return NoContent();
        }
    }
}
