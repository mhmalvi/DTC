using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.Core.Models.Enums;
using UserModel = DTCBillingSystem.Core.Models.User;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Service interface for user management operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        Task<AuthenticationResponse> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Register a new user
        /// </summary>
        Task<RegistrationResponse> RegisterUserAsync(string username, string email, string password, UserRole role);

        /// <summary>
        /// Change user's password
        /// </summary>
        Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<UserModel?> GetUserByIdAsync(int userId);

        /// <summary>
        /// Get user by username
        /// </summary>
        Task<UserModel?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Reset user's password
        /// </summary>
        Task<PasswordResetResponse> ResetPasswordAsync(string username);
    }
} 