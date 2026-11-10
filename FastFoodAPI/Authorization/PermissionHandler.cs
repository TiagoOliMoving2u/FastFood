
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FastFoodAPI.Authorization;

public class PermissionHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly PermissionService _permissionService;

    public PermissionHandler(
        PermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 1. Verificar se existe um utilizador autenticado
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        // 2. Recuperar o UserId do JWT
        var userIdClaim =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out int userId)
            || userId <= 0)
        {
            return;
        }

        // 3. Consultar as permissões na base de dados
        bool allowed = await _permissionService.HasPermissionAsync(
            userId,
            requirement.Code);

        // 4. Autorizar caso a permissão exista
        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
