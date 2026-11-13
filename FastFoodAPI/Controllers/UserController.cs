
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastFoodAPI.DTOs;

namespace FastFoodAPI.Controllers;

public record LoginRequest(
    string Username,
    string Password);

[ApiController]
[Route("api/user")]
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

    [Authorize(Policy = "CanViewUsers")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
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

        Console.Write($"Authenticated user: {user?.Username}");

        if (user is null)
        {
            return Unauthorized(new
            {
                Message = "Username ou password inválidos."
            });
        }
        Console.WriteLine("Passou");


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


    [Authorize(Policy = "CanCreateUsers")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        if (!_environment.IsDevelopment() ||
            HttpContext.Connection.RemoteIpAddress is not { } ip ||
            !System.Net.IPAddress.IsLoopback(ip))
        {
            return NotFound();
        }
        var user = await _userService.CreateUserAsync(
            request.Username,
            request.Password,
            request.Role);
        if (user is null)
        {
            return BadRequest(new
            {
                Message = "Não foi possível criar o usuário."
            });
        }
        return Ok(new
        {
            Message = "Usuário criado com sucesso.",
            //Username = user.Username,
            //Role = user.Role
        });
    }

    [Authorize(Policy = "CanManageUsers")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser(
        [FromBody] UpdateUserDetailsRequest request)
    {
        var success = await _userService.UpdateUserDetailsAsync(request);
        if (!success)
        {
            return BadRequest(new
            {
                Message = "Não foi possível atualizar o usuário."
            });
        }
        return Ok(new
        {
            Message = "Usuário atualizado com sucesso."
        });
    }
}
