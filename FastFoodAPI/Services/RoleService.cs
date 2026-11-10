using FastFoodAPI.Models;
using FastFoodAPI.DTOs;
using FastFoodAPI.Repositories;

namespace FastFoodAPI.Services
{
    public class RoleService
    {
        private readonly RoleRepository _roleRepository;

        public RoleService(RoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<List<Role>?> GetRolesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _roleRepository.GetRolesAsync(cancellationToken);
        }

        /*
        public async Task<bool> CreateRoleAsync(
            CreateRoleRequest createRoleRequest,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(createRoleRequest.Name))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(createRoleRequest.Description))
            {
                return false;
            }

            return await _roleRepository.CreateRoleAsync(createRoleRequest, cancellationToken);
        }
        */

        public Task<bool> CreateRoleAsync(
            CreateRoleRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("O nome da Role é obrigatório.");

            request.Name = request.Name.Trim();
            request.Description = request.Description.Trim();

            if (request.Name.Length > 50)
                throw new ArgumentException(
                    "O nome da Role não pode ultrapassar 50 caracteres.");

            if (request.Description?.Length > 200)
                throw new ArgumentException(
                    "A descrição não pode ultrapassar 200 caracteres.");

            if (request.PermissionIds?.Any(id => id <= 0) == true)
                throw new ArgumentException(
                    "Os identificadores das permissões são inválidos.");

            return _roleRepository.CreateRoleAsync(
                request,
                cancellationToken);
        }

        public Task<IReadOnlyList<int>?> GetRolePermissionIdsAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            if (roleId <= 0)
                throw new ArgumentException("Invalid RoleId.");

            return _roleRepository.GetRolePermissionIdsAsync(
                roleId,
                cancellationToken);
        }

        public Task<bool> UpdateRolePermissionsAsync(
            int roleId,
            UpdateRolePermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (roleId <= 0)
                throw new ArgumentException("Invalid RoleId.");

            if (request?.PermissionIds is null)
                throw new ArgumentException(
                    "PermissionIds é obrigatório.");

            if (request.PermissionIds.Any(id => id <= 0))
                throw new ArgumentException(
                    "Uma ou mais permissões são inválidas.");

            return _roleRepository.UpdateRolePermissionsAsync(
                roleId,
                request.PermissionIds,
                cancellationToken);
        }
    }
}
