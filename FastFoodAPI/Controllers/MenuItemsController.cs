using FastFoodAPI.DTOs;
using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/MenuItems")]
    public class MenuItemsController : ControllerBase
    {
        private readonly MenuItemRepository menuItemRepository;

        public MenuItemsController(MenuItemRepository menuItemRepository)
        {
            this.menuItemRepository = menuItemRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems(
            [FromQuery] int? id = null,
            [FromQuery] int[]? ids = null,
            CancellationToken cancellationToken = default)
        {
            var menuItems = await menuItemRepository.GetMenuItemsAsync(id, ids, cancellationToken);
            
            return Ok(menuItems);
        }

        

    }
}
