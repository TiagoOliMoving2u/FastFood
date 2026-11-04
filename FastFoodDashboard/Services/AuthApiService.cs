using FastFoodDashboard.Models;
using System.Net;
using System.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FastFoodDashboard.Services
{
    public class AuthApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
        {
                var client = _httpClientFactory.CreateClient("FastFoodApi");

                var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);
            
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
                "api/auth/me");

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
    }
}
