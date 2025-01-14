using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Authentication;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UserService(
            ILogger<UserService> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<LoginResult> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await _unitOfWork.Users.FindAsync(u => u.Username == username);
                if (user == null)
                {
                    return new LoginResult { Success = false, Message = "Invalid username or password" };
                }

                if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    return new LoginResult { Success = false, Message = "Invalid username or password" };
                }

                if (!user.IsActive)
                {
                    return new LoginResult { Success = false, Message = "Account is deactivated" };
                }

                // TODO: Generate JWT token
                return new LoginResult
                {
                    Success = true,
                    Message = "Authentication successful",
                    UserId = user.Id,
                    Username = user.Username,
                    UserRole = user.Role
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Username}", username);
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found", nameof(userId));
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetUsersAsync(UserRole? role = null)
        {
            try
            {
                if (role.HasValue)
                {
                    return await _unitOfWork.Users.FindAsync(u => u.Role == role.Value);
                }
                return await _unitOfWork.Users.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                throw;
            }
        }

        public async Task<UserRegistrationResult> RegisterUserAsync(User user, string password)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.FindAsync(u => u.Username == user.Username);
                if (existingUser != null)
                {
                    return new UserRegistrationResult { Success = false, Message = "Username already exists" };
                }

                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                return new UserRegistrationResult
                {
                    Success = true,
                    Message = "User registered successfully",
                    UserId = user.Id,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", user.Username);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var userId = await _currentUserService.GetCurrentUserIdAsync();
                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (!VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    return false;
                }

                CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.LastModifiedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", await _currentUserService.GetCurrentUserIdAsync());
                throw;
            }
        }

        public async Task<PasswordResetResult> ResetPasswordAsync(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.FindAsync(u => u.Username == username);
                if (user == null)
                {
                    return new PasswordResetResult { Success = false, Message = "User not found" };
                }

                var newPassword = GenerateRandomPassword();
                CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.LastModifiedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return new PasswordResetResult
                {
                    Success = true,
                    Message = "Password reset successful",
                    NewPassword = newPassword
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {Username}", username);
                throw;
            }
        }

        public async Task<bool> UpdateUserProfileAsync(User user)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    return false;
                }

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.LastModifiedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.Role = newRole;
                user.LastModifiedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = false;
                user.LastModifiedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                throw;
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
                return true;
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();
            for (int i = 0; i < 12; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            return password.ToString();
        }
    }
} 