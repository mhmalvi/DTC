using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

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
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            if (!user.IsActive)
            {
                return new AuthenticationResponse { Success = false, Message = "Account is deactivated" };
            }

            var (storedHash, storedSalt) = (user.PasswordHash, user.PasswordSalt);
            if (!_passwordHasher.VerifyPassword(password, storedHash, storedSalt))
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            
            await _auditService.LogAsync("User", user.Id.ToString(), user.Id, AuditAction.Login.ToString());

            return new AuthenticationResponse
            {
                Success = true,
                Message = "Authentication successful",
                Username = user.Username,
                Role = user.Role,
                RequirePasswordChange = user.RequirePasswordChange,
                User = user
            };
        }

        public async Task<RegistrationResponse> RegisterUserAsync(string username, string email, string password, UserRole role)
        {
            if (!await _unitOfWork.Users.IsUsernameUniqueAsync(username))
            {
                return new RegistrationResponse { Success = false, Message = "Username already exists" };
            }

            if (!await _unitOfWork.Users.IsEmailUniqueAsync(email))
            {
                return new RegistrationResponse { Success = false, Message = "Email already exists" };
            }

            var (hash, salt) = _passwordHasher.HashPassword(password);
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = username,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = username
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync("User", user.Id.ToString(), user.Id, AuditAction.Create.ToString());

            return new RegistrationResponse { Success = true, Message = "User registered successfully" };
        }

        public async Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return new PasswordChangeResponse { Success = false, Message = "User not found" };
            }

            if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return new PasswordChangeResponse { Success = false, Message = "Current password is incorrect" };
            }

            var (hash, salt) = _passwordHasher.HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.RequirePasswordChange = false;
            user.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync("User", user.Id.ToString(), user.Id, AuditAction.PasswordChange.ToString(), "Password changed");

            return new PasswordChangeResponse { Success = true, Message = "Password changed successfully" };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetByUsernameAsync(username);
        }

        public async Task<PasswordResetResponse> ResetPasswordAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null)
            {
                return new PasswordResetResponse { Success = false, Message = "User not found" };
            }

            var tempPassword = GenerateTemporaryPassword();
            var (hash, salt) = _passwordHasher.HashPassword(tempPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.RequirePasswordChange = true;
            user.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync("User", user.Id.ToString(), user.Id, AuditAction.PasswordReset.ToString(), "Password reset");

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Password has been reset",
                NewPassword = tempPassword
            };
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            var tempPassword = new char[12];
            for (var i = 0; i < tempPassword.Length; i++)
            {
                tempPassword[i] = chars[random.Next(chars.Length)];
            }
            return new string(tempPassword);
        }
    }
} 