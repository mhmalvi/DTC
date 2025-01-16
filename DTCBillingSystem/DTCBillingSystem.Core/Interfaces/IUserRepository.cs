using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Repository interface for User entity operations
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Get user by username
        /// </summary>
        Task<User?> FindByUsernameAsync(string username);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<User?> FindByEmailAsync(string email);

        /// <summary>
        /// Check if username is available
        /// </summary>
        Task<bool> IsUsernameUniqueAsync(string username);

        /// <summary>
        /// Check if email is available
        /// </summary>
        Task<bool> IsEmailUniqueAsync(string email);

        /// <summary>
        /// Get active users
        /// </summary>
        Task<IEnumerable<User>> GetActiveUsersAsync();

        /// <summary>
        /// Get users by role
        /// </summary>
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);

        /// <summary>
        /// Update last login
        /// </summary>
        Task<bool> UpdateLastLoginAsync(int userId, string? ipAddress = null);

        /// <summary>
        /// Update login attempts
        /// </summary>
        Task<bool> UpdateLoginAttemptsAsync(int userId, int attempts);

        /// <summary>
        /// Lock user
        /// </summary>
        Task<bool> LockUserAsync(int userId, System.DateTime lockUntil);

        /// <summary>
        /// Unlock user
        /// </summary>
        Task<bool> UnlockUserAsync(int userId);
    }
} 