
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FastFoodAPI.Controllers;

public record LoginRequest(
    string Username,
    string Password);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly SymmetricSecurityKey _signingKey;

    public AuthController(
        IConfiguration configuration,
        IHostEnvironment environment,
        SymmetricSecurityKey signingKey)
    {
        _configuration = configuration;
        _environment = environment;
        _signingKey = signingKey;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!_environment.IsDevelopment() ||
            HttpContext.Connection.RemoteIpAddress is not { } ip ||
            !System.Net.IPAddress.IsLoopback(ip))
        {
            return NotFound();
        }


        var account = _configuration
        .GetSection("DevAuth:Users")
        .GetChildren()
        .FirstOrDefault(user =>
            user["Username"] == request.Username);

        if (account is null)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        var username = account["Username"];
        var password = account["Password"];
        var role = account["Role"];

        // Verificar credenciais
        var passwordValid =
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(request.Password ?? "")),
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(password!)));

        if (request.Username != username || !passwordValid)
        {
            return Unauthorized("Credenciais inválidas.");
        }

        var expires = DateTime.UtcNow.AddMinutes(15);

        // Dados incluídos no JWT
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new Claim("role", role!)
        };

        // Assinar o JWT
        var credentials = new SigningCredentials(
            _signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            AccessToken = tokenString,
            TokenType = "Bearer",
            ExpiresAtUtc = expires,
            Role = role,
            Username = username
        });
    }
}
