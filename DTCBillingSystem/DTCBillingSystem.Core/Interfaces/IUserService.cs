using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for user management and authentication operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticate user and return auth token
        /// </summary>
        Task<AuthenticationResult> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Register a new user
        /// </summary>
        Task<User> RegisterUserAsync(
            string username,
            string password,
            string fullName,
            string email,
            string phoneNumber,
            UserRole role);

        /// <summary>
        /// Change user's password
        /// </summary>
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Reset user's password and send temporary password
        /// </summary>
        Task<string> ResetPasswordAsync(string email);

        /// <summary>
        /// Update user profile
        /// </summary>
        Task UpdateUserProfileAsync(
            int userId,
            string fullName,
            string email,
            string phoneNumber);

        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<User> GetUserByIdAsync(int userId);

        /// <summary>
        /// Get user by username
        /// </summary>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Get all users with optional role filter
        /// </summary>
        Task<IEnumerable<User>> GetUsersAsync(UserRole? role = null);

        /// <summary>
        /// Update user's role
        /// </summary>
        Task UpdateUserRoleAsync(int userId, UserRole newRole);

        /// <summary>
        /// Deactivate user account
        /// </summary>
        Task DeactivateUserAsync(int userId);

        /// <summary>
        /// Activate user account
        /// </summary>
        Task ActivateUserAsync(int userId);

        /// <summary>
        /// Lock user account
        /// </summary>
        Task LockUserAsync(int userId, TimeSpan duration);

        /// <summary>
        /// Unlock user account
        /// </summary>
        Task UnlockUserAsync(int userId);

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, string permission);
    }

    /// <summary>
    /// Result of authentication attempt
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public User User { get; set; }
        public string ErrorMessage { get; set; }
    }
} 