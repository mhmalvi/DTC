using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, string password, int createdByUserId);
        Task UpdateUserAsync(User user, int updatedByUserId);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, int updatedByUserId);
        Task ResetPasswordAsync(int userId, string newPassword, int updatedByUserId);
        Task<bool> ValidateCredentialsAsync(string username, string password);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByIdAsync(int userId);
    }
} 