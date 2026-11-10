using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastFoodAPI.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    
    public class PermissionsController : ControllerBase
    {
        private readonly PermissionRepository _permissionRepository;

        public PermissionsController(
            PermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken)
        {
            var permissions =
                await _permissionRepository.GetPermissionsAsync(
                    cancellationToken);

            return Ok(permissions);
        }

    }
}
