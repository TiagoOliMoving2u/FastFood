using FastFoodDashboard.Models;
using System.Net.Http.Headers;

namespace FastFoodDashboard.Services
{
    public class DashboardApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DashboardApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyList<Dashboard>> GetAllDashboards(string token,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("FastFoodApi");
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/dashboards");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(
                request, cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Dashboard>>() ?? new List<Dashboard>();
        }
    }
}
