using FastFoodDashboard.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Net.Http.Headers;

namespace FastFoodDashboard.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyList<UserResponse>?> GetAllUsersAsync(string token,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("FastFoodApi");
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/user");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(
                request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<List<UserResponse>>() ?? new List<UserResponse>();
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
        {
                var client = _httpClientFactory.CreateClient("FastFoodApi");

                var response = await client.PostAsJsonAsync("api/user/login", loginRequest);
            
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        string token,
        CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("FastFoodApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/user/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(
                request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<CurrentUserResponse>(
                    cancellationToken: cancellationToken);
        }

        public async Task<bool> CreateUserAsync(
            string token,
            CreateUserRequest createUserRequest,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("FastFoodApi");
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/user/create");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            
            request.Content = JsonContent.Create(createUserRequest);
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

        public async Task<HttpStatusCode> UpdateUserDetailsAsync(
            string token,
            UpdateUserDetailsRequest updateUserDetailsRequest,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("FastFoodApi");
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                "api/user/update");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(updateUserDetailsRequest);
            var response = await client.SendAsync(
                request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                return response.StatusCode;
            }
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }
    }
}
