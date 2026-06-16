using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.Core.Models.Enums;

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
        Task<LoginResult> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Register a new user
        /// </summary>
        Task<UserRegistrationResult> RegisterUserAsync(
            string username,
            string password,
            string fullName,
            string email,
            string phoneNumber,
            UserRole role);

        /// <summary>
        /// Change user's password
        /// </summary>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Reset user's password and send temporary password
        /// </summary>
        Task<PasswordResetResult> ResetPasswordAsync(string username);

        /// <summary>
        /// Update user profile
        /// </summary>
        Task<bool> UpdateUserProfileAsync(
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
        Task<bool> UpdateUserRoleAsync(int userId, UserRole newRole);

        /// <summary>
        /// Deactivate user account
        /// </summary>
        Task<bool> DeactivateUserAsync(int userId);

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, string permission);
    }
} 