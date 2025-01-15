using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Authentication;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;

        public UserService(
            IUserRepository userRepository,
            IAuditService auditService)
        {
            _userRepository = userRepository;
            _auditService = auditService;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResponse { Success = false, Message = "Account is deactivated" };
            }

            var passwordHash = HashPassword(password, user.PasswordSalt);
            if (!passwordHash.SequenceEqual(user.PasswordHash))
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _auditService.LogActionAsync("User", user.Id, "Login", $"User {username} logged in");

            return new AuthenticationResponse
            {
                Success = true,
                Message = "Authentication successful",
                User = user,
                Username = user.Username,
                Role = user.Role,
                RequirePasswordChange = user.RequirePasswordChange
            };
        }

        public async Task<RegistrationResponse> RegisterUserAsync(string username, string email, string password, UserRole role)
        {
            if (await _userRepository.GetByUsernameAsync(username) != null)
                return new RegistrationResponse { Success = false, Message = "Username already exists" };

            if (await _userRepository.GetByEmailAsync(email) != null)
                return new RegistrationResponse { Success = false, Message = "Email already exists" };

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(password, salt);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _auditService.LogActionAsync("User", user.Id, "Register", $"Registered new user {username}");

            return new RegistrationResponse { Success = true, Message = "User registered successfully" };
        }

        public async Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new PasswordChangeResponse { Success = false, Message = "User not found" };

            var currentPasswordHash = HashPassword(currentPassword, user.PasswordSalt);
            if (!currentPasswordHash.SequenceEqual(user.PasswordHash))
                return new PasswordChangeResponse { Success = false, Message = "Current password is incorrect" };

            var newSalt = GenerateSalt();
            var newPasswordHash = HashPassword(newPassword, newSalt);

            user.PasswordHash = newPasswordHash;
            user.PasswordSalt = newSalt;
            user.LastModifiedAt = DateTime.UtcNow;
            user.RequirePasswordChange = false;

            await _userRepository.UpdateAsync(user);
            await _auditService.LogActionAsync("User", userId, "ChangePassword", "Password changed successfully");

            return new PasswordChangeResponse { Success = true, Message = "Password changed successfully" };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<PasswordResetResponse> ResetPasswordAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return new PasswordResetResponse { Success = false, Message = "User not found" };

            var tempPassword = GenerateTemporaryPassword();
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(tempPassword, salt);

            user.PasswordHash = hashedPassword;
            user.PasswordSalt = salt;
            user.RequirePasswordChange = true;
            user.LastModifiedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _auditService.LogActionAsync("User", user.Id, "ResetPassword", "Password reset");

            return new PasswordResetResponse 
            { 
                Success = true, 
                Message = "Password reset successful",
                NewPassword = tempPassword 
            };
        }

        private byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            var tempPassword = new char[12];
            for (int i = 0; i < tempPassword.Length; i++)
            {
                tempPassword[i] = chars[random.Next(chars.Length)];
            }
            return new string(tempPassword);
        }
    }
} 