
using FastFoodDashboard.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FastFoodDashboard.Services;

public class RoleApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RoleApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<RoleDto>?> GetRolesAsync(
            string token,
            CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/roles?inactiveToo=true");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(
            request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            return new List<RoleDto>();
        }

        var content = await response.Content.ReadAsStringAsync(
        cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<RoleDto>>(
                cancellationToken: cancellationToken);
    }

    /*
    public async Task<bool> CreateRoleAsync(
            string token,
            CreateRoleRequest createRoleRequest,
            CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/roles/create");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(createRoleRequest);
        var response = await client.SendAsync(
            request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden)
        {
            return false;
        }
        response.EnsureSuccessStatusCode();
        return true;
    }
    */

    public async Task<List<PermissionResponse>> GetPermissionsAsync(
        string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get, "api/permissions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PermissionResponse>>()
            ?? new();
    }

    public async Task<HttpStatusCode> CreateRoleAsync(
        CreateRoleRequest role,
        string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Post, "api/roles")
        {
            Content = JsonContent.Create(role)
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    public async Task<RolePermissionsResponse> GetRolePermissionsAsync(
    int roleId,
    string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/roles/{roleId}/permissions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<RolePermissionsResponse>()
            ?? throw new InvalidOperationException(
                "Resposta inválida da API.");
    }

    public async Task<HttpStatusCode> UpdateRolePermissionsAsync(
        int roleId,
        List<int> permissionIds,
        string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/roles/{roleId}/permissions")
        {
            Content = JsonContent.Create(new
            {
                PermissionIds = permissionIds
            })
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    public async Task<HttpStatusCode> UpdateStateRoleAsync(
        int roleId,
        int isActive,
        string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/roles/{roleId}/state")
        {
            Content = JsonContent.Create(new
            {
                Id = roleId,
                IsActive = isActive
            })
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        return response.StatusCode;
    }

    public async Task<HttpStatusCode> UpdateRoleDetailsAsync(
        int roleId,
        string name,
        string description,
        string token)
    {
        var client = _httpClientFactory.CreateClient("FastFoodApi");
        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/roles/{roleId}/details")
        {
            Content = JsonContent.Create(new
            {
                Id = roleId,
                Name = name,
                Description = description
            })
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.SendAsync(request);
        return response.StatusCode;
    }
}
