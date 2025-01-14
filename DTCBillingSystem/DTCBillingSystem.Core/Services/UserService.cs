using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public UserService(
            ILogger<UserService> logger,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<User> CreateUserAsync(User user, string password, int createdByUserId)
        {
            try
            {
                if (await _unitOfWork.Users.AnyAsync(u => u.Username == user.Username))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                user.PasswordHash = HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;
                user.LastLoginAt = null;

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogCreateAsync(user, createdByUserId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", user.Username);
                throw;
            }
        }

        public async Task UpdateUserAsync(User user, int updatedByUserId)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new ArgumentException("User not found", nameof(user));
                }

                if (user.Username != existingUser.Username &&
                    await _unitOfWork.Users.AnyAsync(u => u.Username == user.Username))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Role = user.Role;
                existingUser.IsActive = user.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogUpdateAsync(existingUser, updatedByUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", user.Id);
                throw;
            }
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, int updatedByUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found", nameof(userId));
                }

                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }

                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogUpdateAsync(user, updatedByUserId, "Password changed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }

        public async Task ResetPasswordAsync(int userId, string newPassword, int updatedByUserId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found", nameof(userId));
                }

                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogUpdateAsync(user, updatedByUserId, "Password reset");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Username == username);
                if (user == null || !user.IsActive)
                {
                    return false;
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return false;
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credentials for user {Username}", username);
                throw;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                throw;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _unitOfWork.Users.GetByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID {UserId}", userId);
                throw;
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
} 