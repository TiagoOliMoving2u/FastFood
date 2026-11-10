using FastFoodAPI.Services;
using FastFoodAPI.DTOs;
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

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRole(
            [FromBody] CreateRoleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var created = await _roleService.CreateRoleAsync(
                    request,
                    cancellationToken);

                if (!created)
                    return Conflict("Já existe uma Role com esse nome.");

                return StatusCode(
                    StatusCodes.Status201Created,
                    new { Message = "Role criada com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
