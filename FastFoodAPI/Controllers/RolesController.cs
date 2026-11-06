using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {

        private readonly RoleService _roleService;
        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetRolesAsync(
            CancellationToken cancellationToken = default)
        {
            var roles = await _roleService.GetRolesAsync(cancellationToken);
            if (roles is null || !roles.Any())
            {
                return NotFound(new
                {
                    Message = "Nenhuma role encontrada."
                });
            }
            return Ok(roles);
        }
    }
}
