using FastFoodDashboard.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;

namespace FastFoodDashboard.Services;

public record StoredJwt(
    string AccessToken,
    DateTime ExpiresAtUtc);

public class BrowserAuthStorage
{
    private const string Key = "fastfood.auth";

    private readonly ProtectedLocalStorage _storage;

    public BrowserAuthStorage(ProtectedLocalStorage storage)
    {
        _storage = storage;
    }

    public async Task SaveAsync(LoginResponse login)
    {
        var data = new StoredJwt(
            login.AccessToken,
            login.ExpiresAtUtc);

        await _storage.SetAsync(Key, data);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result =
                await _storage.GetAsync<StoredJwt>(Key);

            if (!result.Success || result.Value is null)
                return null;

            if (result.Value.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await _storage.DeleteAsync(Key);
                return null;
            }

            return result.Value.AccessToken;
        }
        catch (CryptographicException)
        {
            // Dados alterados ou impossíveis de desencriptar
            await _storage.DeleteAsync(Key);
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _storage.DeleteAsync(Key);
    }
}