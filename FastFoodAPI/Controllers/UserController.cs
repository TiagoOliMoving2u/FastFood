
using FastFoodAPI.Services;
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
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly UserService _userService;

    public UserController(
        IConfiguration configuration,
        IHostEnvironment environment,
        SymmetricSecurityKey signingKey,
        UserService userService)
    {
        _configuration = configuration;
        _environment = environment;
        _signingKey = signingKey;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!_environment.IsDevelopment() ||
            HttpContext.Connection.RemoteIpAddress is not { } ip ||
            !System.Net.IPAddress.IsLoopback(ip))
        {
            return NotFound();
        }


        var user = await _userService.AuthenticateAsync(
            request.Username,
            request.Password);

        if (user is null)
        {
            return Unauthorized(new
            {
                Message = "Username ou password inválidos."
            });
        }

        

        var expires = DateTime.UtcNow.AddMinutes(15);

        // Dados incluídos no JWT
        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username),

            new Claim(
                ClaimTypes.Role,
                user.Role),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
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
            Role = user.Role,
            Username = user.Username
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Username = User.FindFirstValue(ClaimTypes.Name),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

}
