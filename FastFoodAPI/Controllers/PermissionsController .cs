using FastFoodAPI.Repositories;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize]

    public class PermissionsController : ControllerBase
    {
        private readonly PermissionRepository _permissionRepository;
        private readonly PermissionService _permissionService;

        public PermissionsController(
            PermissionRepository permissionRepository,
            PermissionService permissionService)
        {
            _permissionRepository = permissionRepository;
            _permissionService = permissionService;
        }

        [Authorize(Policy = "CanViewRoles")]
        [HttpGet]
        public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken)
        {
            var permissions =
                await _permissionRepository.GetPermissionsAsync(
                    cancellationToken);

            return Ok(permissions);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyPermissions(
            CancellationToken cancellationToken)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(userIdClaim, out int userId) || userId <= 0)
                return Unauthorized();

            var permissions =
                await _permissionService.GetUserPermissionsAsync(
                    userId,
                    cancellationToken);

            return Ok(permissions);
        }

    }
}
