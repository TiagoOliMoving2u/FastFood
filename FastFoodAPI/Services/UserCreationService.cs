using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FastFoodAPI.Services
{
    public class UserCreationService
    {
        private readonly UserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserCreationService(
        UserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<int?> CreateUserAsync(
            string username,
            string password,
            string roleName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(
                    "Username, password e role são obrigatórios.");
            }

            if (password.Length < 12)
            {
                throw new ArgumentException(
                    "A password deve ter pelo menos 12 caracteres.");
            }

            var user = new User
            {
                Username = username.Trim(),
                IsActive = true
            };

            // Criar o hash da password
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                password);

            // Guardar na base de dados
            return await _userRepository.CreateAsync(
                user,
                roleName,
                cancellationToken);
        }
    }
}
