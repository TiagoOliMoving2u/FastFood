using Microsoft.AspNetCore.Mvc;
using FastFoodAPI.Services;

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
        [HttpGet]
        public async Task<IActionResult> GetDashboards(CancellationToken cancellationToken)
        {
            var dashboards = await _dashboardService.GetAllDashboards(cancellationToken);
            return Ok(dashboards);
        }
    }
}
