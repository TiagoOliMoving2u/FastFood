using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/dashboards")]
    public class DashboardsController : ControllerBase
    {
        private readonly DashboardService _dashboardService;
        public DashboardsController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetDashboards(CancellationToken cancellationToken)
        {
            var dashboards = await _dashboardService.GetAllDashboards(User,cancellationToken);
            return Ok(dashboards);
        }
    }
}
