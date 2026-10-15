using FastFoodAPI.Data;
using Microsoft.EntityFrameworkCore;
using FastFoodAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<SqlConnectionFactory>();

builder.Services.AddScoped<AreaRepository>();
builder.Services.AddScoped<OrderRepository>();

// Entity Framework Core
builder.Services.AddDbContext<FastFoodDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration.GetConnectionString(
                "DefaultConnection")
        )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
