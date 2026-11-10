using FastFoodDashboard.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using FastFoodDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "API BaseUrl não configurada.");

builder.Services.AddHttpClient("FastFoodApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<BrowserAuthStorage>();
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<RoleApiService>();
builder.Services.AddScoped<PermissionApiService>();
// Adicionar o serviço SignalRService como transient para que seja criado uma nova instância a cada injeção de dependência
builder.Services.AddTransient<SignalRService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
