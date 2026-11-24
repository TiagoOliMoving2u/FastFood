using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using System.Security.Claims;

namespace FastFoodAPI.Services
{
    public class DashboardService
    {
        private readonly PermissionService _permissionService;
        private readonly DashboardRepository _dashboardRepository;
        public DashboardService(DashboardRepository dashboardRepository, PermissionService permissionService)
        {
            _dashboardRepository = dashboardRepository;
            _permissionService = permissionService;
        }
        public async Task<List<Dashboard>> GetAllDashboards(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            if (user.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException(
                    "User is not authenticated.");

            var userIdClaim =
                user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                throw new UnauthorizedAccessException(
                    "Invalid user identity.");

            var dashboards = await _dashboardRepository.GetAllDashboards(cancellationToken);

            var selectedDashboards = new List<Dashboard>();

            var permissions = await _permissionService.GetUserPermissionsAsync(
                    userId, cancellationToken);

            var userPermissions = permissions.ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var dashboard in dashboards)
            {
                if (dashboard.IsActive != 1)
                    continue;

                // sem permissoes, publico
                if (dashboard.PermissionsRequired.Count == 0)
                {
                    selectedDashboards.Add(dashboard);
                    continue;
                }

                var hasAccess = dashboard.PermissionsRequired.Any(permission =>
                    (
                        permission.Code.EndsWith(
                            ".view", StringComparison.OrdinalIgnoreCase)
                        ||
                        permission.Code.EndsWith(
                            ".manage", StringComparison.OrdinalIgnoreCase)
                    )
                    && userPermissions.Contains(permission.Code));

                if (hasAccess)
                    selectedDashboards.Add(dashboard);
            }

            return selectedDashboards;
        }

        public async Task<bool> CreateDashboardAsync(CreateDashboardRequest dashboard, CancellationToken cancellationToken)
        {
            if (dashboard == null)
                throw new ArgumentNullException(nameof(dashboard));
            // Validate required fields
            if (string.IsNullOrWhiteSpace(dashboard.Name))
                throw new ArgumentException("Dashboard name is required.");
            if (string.IsNullOrWhiteSpace(dashboard.Slug))
                throw new ArgumentException("Dashboard slug is required.");
            // Additional validation logic can be added here
            var createdDashboard = await _dashboardRepository.CreateDashboardAsync(dashboard, cancellationToken);
            return true;
        }
    }
}
