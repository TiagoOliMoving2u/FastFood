using FastFoodAPI.Models;
using FastFoodAPI.Repositories;
using Microsoft.AspNetCore.Identity;

namespace FastFoodAPI.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            UserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await _userRepository.GetByUsernameAsync(
                username,
                cancellationToken);

            if (user is null)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }

    }
}
