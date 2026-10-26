using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FastFoodAPI.Hubs;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly OrderRepository _orderRepository;
        private readonly IHubContext<OrdersHub> _hubContext;
        public SystemController(OrderRepository orderRepository,
            IHubContext<OrdersHub> hubContext)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        [Authorize(Policy = "CanResetSystem")]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetAllAsync(CancellationToken cancellationToken = default)
        {
            await _orderRepository.ResetAllAsync(cancellationToken);

            await _hubContext.Clients.All.SendAsync(
                "OrdersChanged",
                cancellationToken);

            return NoContent();
        }
    }
}
