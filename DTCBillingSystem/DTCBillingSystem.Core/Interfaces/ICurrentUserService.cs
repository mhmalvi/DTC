using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for accessing current user information
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the ID of the currently authenticated user
        /// </summary>
        Task<int> GetCurrentUserIdAsync();

        /// <summary>
        /// Gets the username of the currently authenticated user
        /// </summary>
        Task<string> GetCurrentUsernameAsync();

        /// <summary>
        /// Gets the role of the currently authenticated user
        /// </summary>
        Task<string> GetCurrentUserRoleAsync();

        /// <summary>
        /// Checks if the current user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(string permission);

        /// <summary>
        /// Gets whether the current user is authenticated
        /// </summary>
        Task<bool> IsAuthenticatedAsync();

        int UserId { get; }
        string Username { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
    }
} 