using FastFoodAPI.Models;
using FastFoodAPI.Repositories;

namespace FastFoodAPI.Services
{
    public class RoleService
    {
        private readonly UserRepository _userRepository;

        public RoleService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<Role>?> GetRolesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetRolesAsync(cancellationToken);
        }
    }
}
