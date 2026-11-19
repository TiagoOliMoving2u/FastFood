using Dapper;
using FastFoodAPI.Data;
using FastFoodAPI.Models;
using System.Security.Claims;

namespace FastFoodAPI.Repositories
{
    public class DashboardRepository
    {
        private SqlConnectionFactory _factory;
        
        public DashboardRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<Dashboard>> GetAllDashboards(CancellationToken cancellationToken)
        {
            using var connection = _factory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT d.Id, d.Name, d.Slug, d.IsActive ,d.AreaId
                FROM Dashboards as d;
                """;
            var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

            var dashboards = await connection.QueryAsync<Dashboard>(command);

            const string sql2 = """
                SELECT ds.Id, ds.DashboardId, ds.State
                FROM DashboardStates as ds
                ORDER BY ds.DashboardId, ds.State;
                """;

            var command2 = new CommandDefinition(sql2, cancellationToken: cancellationToken);

            var showStates = await connection.QueryAsync<DashboardState>(command2);

            var statesByDashId = showStates.ToLookup(s => s.DashboardId);

            foreach (var dashboard in dashboards)
            {
                dashboard.ShowStates = statesByDashId[dashboard.Id].ToList();
            }

            const string sql3 = """
                SELECT dp.Id, dp.DashboardId, dp.PermissionId, p.Code
                FROM DashboardPermissions as dp
                INNER JOIN Permissions as p ON p.Id = dp.PermissionId
                ORDER BY dp.DashboardId;
                """;

            var command3 = new CommandDefinition(sql3, cancellationToken: cancellationToken);

            var permission = await connection.QueryAsync<DashboardPermission>(command3);

            var permissionsByDashId = permission.ToLookup(p => p.DashboardId);

            foreach (var dashboard in dashboards)
            {
                dashboard.PermissionsRequired = permissionsByDashId[dashboard.Id].ToList();
            }

            return dashboards.ToList();
        }
    }
}
