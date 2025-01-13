using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IAuditService _auditService;

        public UserService(
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await _unitOfWork.UsersExt.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: User {Username} not found", username);
                    return null;
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Authentication failed: User {Username} is inactive", username);
                    return null;
                }

                if (user.IsLocked && user.LockoutEnd > DateTime.UtcNow)
                {
                    _logger.LogWarning("Authentication failed: User {Username} is locked until {LockoutEnd}", 
                        username, user.LockoutEnd);
                    return null;
                }

                var passwordHash = HashPassword(password, user.PasswordSalt);
                var isValid = await _unitOfWork.UsersExt.ValidateCredentialsAsync(username, passwordHash);

                if (!isValid)
                {
                    await _unitOfWork.UsersExt.IncrementFailedLoginAttemptsAsync(user.Id);
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogWarning("Authentication failed: Invalid password for user {Username}", username);
                    return null;
                }

                // Reset failed attempts on successful login
                if (user.FailedLoginAttempts > 0)
                {
                    await _unitOfWork.UsersExt.ResetFailedLoginAttemptsAsync(user.Id);
                    await _unitOfWork.SaveChangesAsync();
                }

                user.LastLoginDate = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "User",
                    user.Id,
                    AuditAction.LoginAttempt,
                    null,
                    "Successful login"
                );

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {Username}", username);
                throw;
            }
        }

        public async Task<User> CreateUserAsync(
            string username,
            string password,
            string fullName,
            string email,
            string phoneNumber,
            UserRole role)
        {
            try
            {
                // Validate username uniqueness
                if (!await _unitOfWork.UsersExt.IsUsernameUniqueAsync(username))
                {
                    throw new InvalidOperationException($"Username '{username}' is already taken");
                }

                // Validate email uniqueness
                if (!await _unitOfWork.UsersExt.IsEmailUniqueAsync(email))
                {
                    throw new InvalidOperationException($"Email '{email}' is already registered");
                }

                // Generate password salt and hash
                var salt = GeneratePasswordSalt();
                var passwordHash = HashPassword(password, salt);

                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    PasswordSalt = salt,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "User",
                    user.Id,
                    AuditAction.Created,
                    null,
                    $"Created new user: {username} with role: {role}"
                );

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", username);
                throw;
            }
        }

        public async Task UpdateUserAsync(
            int userId,
            string fullName,
            string email,
            string phoneNumber,
            UserRole? role = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Check email uniqueness if changed
                if (email != user.Email && !await _unitOfWork.UsersExt.IsEmailUniqueAsync(email, userId))
                {
                    throw new InvalidOperationException($"Email '{email}' is already registered");
                }

                var oldValues = new
                {
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Role
                };

                user.FullName = fullName;
                user.Email = email;
                user.PhoneNumber = phoneNumber;
                if (role.HasValue)
                {
                    user.Role = role.Value;
                }

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var newValues = new
                {
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Role
                };

                await _auditService.LogActionAsync(
                    "User",
                    userId,
                    AuditAction.Updated,
                    oldValues,
                    newValues
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                throw;
            }
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                // Verify current password
                var currentHash = HashPassword(currentPassword, user.PasswordSalt);
                if (currentHash != user.PasswordHash)
                {
                    throw new InvalidOperationException("Current password is incorrect");
                }

                // Generate new salt and hash
                var newSalt = GeneratePasswordSalt();
                var newHash = HashPassword(newPassword, newSalt);

                user.PasswordHash = newHash;
                user.PasswordSalt = newSalt;

                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "User",
                    userId,
                    AuditAction.PasswordChanged,
                    null,
                    "Password changed successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
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
                    throw new KeyNotFoundException($"User with ID {userId} not found");
                }

                user.IsActive = false;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "User",
                    userId,
                    AuditAction.StatusChanged,
                    "Active",
                    "Inactive"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                throw;
            }
        }

        private string GeneratePasswordSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt);
                var bytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
} 