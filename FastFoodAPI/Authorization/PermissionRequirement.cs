using Microsoft.AspNetCore.Authorization;

namespace FastFoodAPI.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Code { get; }

    public PermissionRequirement(string code)
    {
        Code = code;
    }
}