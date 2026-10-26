using FastFoodAPI.Data;
using FastFoodAPI.Hubs;
using FastFoodAPI.Repositories;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<SqlConnectionFactory>();

builder.Services.AddScoped<AreaRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<MenuItemRepository>();
builder.Services.AddScoped<OrderService>();
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

                RoleClaimType = "role",
                NameClaimType = "sub",

                ClockSkew = TimeSpan.FromSeconds(30)
            };
    });

// Autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanResetSystem", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("KitchenSupervisor");
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

app.MapHub<OrdersHub>("/hubs/orders");

app.Run();
