using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FastFoodAPI.Hubs
{
    [Authorize]
    public class OrdersHub : Hub
    {
    }
}
