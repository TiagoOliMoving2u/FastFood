using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FastFoodDashboard.Services;

public class PermissionApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PermissionApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<string>> GetMyPermissionsAsync(
        string token)
    {
        var client =
            _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/permissions/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<string>>() ?? new();
    }
}