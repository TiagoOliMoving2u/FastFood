using FastFoodAPI.Repositories;

namespace FastFoodAPI.Services;

public class PermissionService
{
    private readonly PermissionRepository _permissionRepository;

    public PermissionService(
        PermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> HasPermissionAsync(
        int userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 ||
            string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        return await _permissionRepository.HasPermissionAsync(
            userId,
            permissionCode,
            cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(
    int userId,
    CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            return [];

        return await _permissionRepository.GetUserPermissionsAsync(
            userId,
            cancellationToken);
    }
}