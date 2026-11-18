using FastFoodAPI.Data;
using FastFoodAPI.Hubs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using FastFoodAPI.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<SqlConnectionFactory>();

builder.Services.AddScoped<AreaRepository>();
builder.Services.AddScoped<MenuItemRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PermissionRepository>();
builder.Services.AddScoped<RoleRepository>(); 
builder.Services.AddScoped<DashboardRepository>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    PermissionHandler>();

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PermissionService>();


builder.Services.AddSignalR();

//Autenticação e Autorização
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer não configurado.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience não configurado.");

var jwtKeyBase64 = builder.Configuration["Jwt:KeyBase64"]
    ?? throw new InvalidOperationException(
        "JWT Key não configurada.");

var keyBytes = Convert.FromBase64String(jwtKeyBase64);

if (keyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "A chave JWT deve ter pelo menos 32 bytes.");
}

var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddSingleton(signingKey);

// Autenticação JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,

                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.HmacSha256
                },
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = "sub",

                ClockSkew = TimeSpan.FromSeconds(30)
            };

        options.Events.OnTokenValidated = async context =>
            {
                var user = context.Principal;

                var userIdClaim =
                    user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user?.FindFirst("sub")?.Value;

                var versionClaim =
                    user?.FindFirst("token_version")?.Value;

                if (!int.TryParse(userIdClaim, out int userId) ||
                    !int.TryParse(versionClaim, out int tokenVersion) ||
                    userId <= 0)
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var repository = context.HttpContext.RequestServices
                    .GetRequiredService<UserRepository>();

                var valid = await repository.IsTokenValidAsync(
                    userId,
                    tokenVersion,
                    context.HttpContext.RequestAborted);

                if (!valid)
                {
                    context.Fail("Sessão inválida ou revogada.");
                }
            };
    });

// Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanResetSystem", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("system.reset"));
    });

    options.AddPolicy("CanViewUsers", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("users.view"));
    });

    options.AddPolicy("CanManageUsers", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("users.manage"));
    });

    options.AddPolicy("CanViewRoles", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("roles.view"));
    });

    options.AddPolicy("CanManageRoles", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new PermissionRequirement("roles.manage"));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//app.MapHub<OrdersHub>("/hubs/orders");

app.MapHub<OrdersHub>("/hubs/orders", options =>
{
    options.CloseOnAuthenticationExpiration = true;
});

app.Run();
