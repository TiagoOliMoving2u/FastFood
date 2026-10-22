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
            [FromBody] int? id = null,
            CancellationToken cancellationToken = default)
        {
            var menuItems = await menuItemRepository.GetMenuItemsAsync(id, cancellationToken);
            if (menuItems == null)
                return NotFound("No menu items found.");
            return Ok(menuItems);
        }

    }
}
