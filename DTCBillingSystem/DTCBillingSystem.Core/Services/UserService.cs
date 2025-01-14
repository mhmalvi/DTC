using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;

        public UserService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _auditService = auditService;
        }

        private DTCBillingSystem.Core.Models.User ConvertToUser(DTCBillingSystem.Core.Models.Entities.User entityUser)
        {
            return new DTCBillingSystem.Core.Models.User
            {
                Id = entityUser.Id,
                Username = entityUser.Username,
                Email = entityUser.Email,
                FirstName = entityUser.FirstName,
                LastName = entityUser.LastName,
                PasswordHash = entityUser.PasswordHash,
                PasswordSalt = entityUser.PasswordSalt,
                Role = entityUser.Role,
                IsActive = entityUser.IsActive,
                LastLoginDate = entityUser.LastLoginDate,
                PhoneNumber = entityUser.PhoneNumber ?? string.Empty,
                RequirePasswordChange = entityUser.RequirePasswordChange,
                CreatedAt = entityUser.CreatedAt,
                CreatedBy = entityUser.CreatedBy,
                LastModifiedAt = entityUser.LastModifiedAt,
                LastModifiedBy = entityUser.LastModifiedBy
            };
        }

        private DTCBillingSystem.Core.Models.Entities.User ConvertToEntityUser(DTCBillingSystem.Core.Models.User user)
        {
            return new DTCBillingSystem.Core.Models.Entities.User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                Role = user.Role,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                PhoneNumber = user.PhoneNumber,
                RequirePasswordChange = user.RequirePasswordChange,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy,
                LastModifiedAt = user.LastModifiedAt,
                LastModifiedBy = user.LastModifiedBy
            };
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            var entityUser = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (entityUser == null)
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            var (storedHash, storedSalt) = (entityUser.PasswordHash, entityUser.PasswordSalt);
            if (!_passwordHasher.VerifyPassword(password, storedHash, storedSalt))
            {
                return new AuthenticationResponse { Success = false, Message = "Invalid username or password" };
            }

            entityUser.LastLoginDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            var user = ConvertToUser(entityUser);
            var token = _tokenService.GenerateToken(user);
            return new AuthenticationResponse
            {
                Success = true,
                Message = "Authentication successful",
                Token = token,
                Username = user.Username,
                Role = user.Role
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
            var user = new DTCBillingSystem.Core.Models.Entities.User
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

            await _auditService.LogAsync("User", user.Id.ToString(), user.Id.ToString(), AuditAction.Create);

            return new RegistrationResponse { Success = true, Message = "User registered successfully" };
        }

        public async Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var entityUser = await _unitOfWork.Users.GetByIdAsync(userId);
            if (entityUser == null)
            {
                return new PasswordChangeResponse { Success = false, Message = "User not found" };
            }

            if (!_passwordHasher.VerifyPassword(currentPassword, entityUser.PasswordHash, entityUser.PasswordSalt))
            {
                return new PasswordChangeResponse { Success = false, Message = "Current password is incorrect" };
            }

            var (hash, salt) = _passwordHasher.HashPassword(newPassword);
            entityUser.PasswordHash = hash;
            entityUser.PasswordSalt = salt;
            entityUser.RequirePasswordChange = false;

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync("User", entityUser.Id.ToString(), entityUser.Id.ToString(), AuditAction.Update, "Password changed");

            return new PasswordChangeResponse
            {
                Success = true,
                Message = "Password changed successfully",
                NewPassword = newPassword
            };
        }

        public async Task<DTCBillingSystem.Core.Models.User?> GetUserByIdAsync(int userId)
        {
            var entityUser = await _unitOfWork.Users.GetByIdAsync(userId);
            return entityUser != null ? ConvertToUser(entityUser) : null;
        }

        public async Task<DTCBillingSystem.Core.Models.User?> GetUserByUsernameAsync(string username)
        {
            var entityUser = await _unitOfWork.Users.GetByUsernameAsync(username);
            return entityUser != null ? ConvertToUser(entityUser) : null;
        }

        public async Task<PasswordResetResponse> ResetPasswordAsync(string username)
        {
            var entityUser = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (entityUser == null)
            {
                return new PasswordResetResponse { Success = false, Message = "User not found" };
            }

            var tempPassword = GenerateTemporaryPassword();
            var (hash, salt) = _passwordHasher.HashPassword(tempPassword);
            entityUser.PasswordHash = hash;
            entityUser.PasswordSalt = salt;
            entityUser.RequirePasswordChange = true;

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync("User", entityUser.Id.ToString(), entityUser.Id.ToString(), AuditAction.Update, "Password reset");

            return new PasswordResetResponse
            {
                Success = true,
                Message = "Password has been reset",
                NewPassword = tempPassword
            };
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
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