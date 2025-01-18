using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Authentication;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuditService _auditService;

        public UserService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return new AuthenticationResponse { Success = false, Message = "User not found" };

            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return new AuthenticationResponse { Success = false, Message = "Invalid password" };

            return new AuthenticationResponse
            {
                Success = true,
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            return users.FirstOrDefault();
        }

        public async Task<RegistrationResponse> RegisterUserAsync(string username, string password, string email, UserRole role)
        {
            var existingUser = await GetUserByUsernameAsync(username);
            if (existingUser != null)
                return new RegistrationResponse { Success = false, Message = "Username already exists" };

            var (hash, salt) = _passwordHasher.HashPassword(password);
            var user = new User
            {
                Username = username,
                Email = email,
                Role = role,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "User",
                "Create",
                user.CreatedBy,
                $"Created user {user.Username}"
            );

            return new RegistrationResponse
            {
                Success = true,
                UserId = user.Id,
                Username = user.Username
            };
        }

        public async Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return new PasswordChangeResponse { Success = false, Message = "User not found" };

            if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
                return new PasswordChangeResponse { Success = false, Message = "Current password is incorrect" };

            var (newHash, newSalt) = _passwordHasher.HashPassword(newPassword);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;
            user.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "User",
                "Update",
                user.LastModifiedBy,
                $"Updated user {user.Username}"
            );

            await _auditService.LogActivityAsync(
                "User",
                "ChangePassword",
                userId,
                $"Changed password for user {user.Username}"
            );

            return new PasswordChangeResponse { Success = true };
        }

        public async Task<PasswordResetResponse> ResetPasswordAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
                return new PasswordResetResponse { Success = false, Message = "User not found" };

            var tempPassword = GenerateTemporaryPassword();
            var (hash, salt) = _passwordHasher.HashPassword(tempPassword);
            
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "User",
                "ResetPassword",
                user.Id,
                $"Reset password for user {user.Username}"
            );

            return new PasswordResetResponse
            {
                Success = true,
                TemporaryPassword = tempPassword
            };
        }

        private string GenerateTemporaryPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
} 