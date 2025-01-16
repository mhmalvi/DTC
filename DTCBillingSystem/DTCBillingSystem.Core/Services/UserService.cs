using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _unitOfWork.Users.FindByUsernameAsync(username);
            if (user == null || user.Status != UserStatus.Active)
                return null;

            if (!_passwordHasher.VerifyPassword(password, user.Password))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            if (await _unitOfWork.Users.FindByUsernameAsync(user.Username) != null)
                throw new InvalidOperationException("Username already exists");

            if (await _unitOfWork.Users.FindByEmailAsync(user.Email) != null)
                throw new InvalidOperationException("Email already exists");

            user.Password = _passwordHasher.HashPassword(password);
            user.Status = UserStatus.Active;
            user.CreatedAt = DateTime.UtcNow;
            user.CreatedBy = "system";

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
            if (existingUser == null)
                throw new InvalidOperationException("User not found");

            if (user.Username != existingUser.Username && 
                await _unitOfWork.Users.FindByUsernameAsync(user.Username) != null)
                throw new InvalidOperationException("Username already exists");

            if (user.Email != existingUser.Email && 
                await _unitOfWork.Users.FindByEmailAsync(user.Email) != null)
                throw new InvalidOperationException("Email already exists");

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Role = user.Role;
            existingUser.Status = user.Status;
            existingUser.UpdatedAt = DateTime.UtcNow;
            existingUser.UpdatedBy = user.UpdatedBy;

            await _unitOfWork.SaveChangesAsync();

            return existingUser;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            if (!_passwordHasher.VerifyPassword(currentPassword, user.Password))
                return false;

            user.Password = _passwordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = user.Username;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Password = _passwordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = "system";

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Status = UserStatus.Inactive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = "system";

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = "system";

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
} 