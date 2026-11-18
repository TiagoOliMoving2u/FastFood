using FastFoodAPI.Models;
using FastFoodAPI.Repositories;

namespace FastFoodAPI.Services
{
    public class DashboardService
    {
        private readonly DashboardRepository _dashboardRepository;
        public DashboardService(DashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }
        public async Task<List<Dashboard>> GetAllDashboards(CancellationToken cancellationToken)
        {
            return await _dashboardRepository.GetAllDashboards(cancellationToken);
        }
    }
}
