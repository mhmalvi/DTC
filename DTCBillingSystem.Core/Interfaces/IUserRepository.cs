using System.Threading.Tasks;
using UserEntity = DTCBillingSystem.Core.Models.Entities.User;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Repository interface for User entity operations
    /// </summary>
    public interface IUserRepository : IRepository<UserEntity>
    {
        /// <summary>
        /// Get user by username
        /// </summary>
        Task<UserEntity?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<UserEntity?> GetByEmailAsync(string email);

        /// <summary>
        /// Check if username is available
        /// </summary>
        Task<bool> IsUsernameUniqueAsync(string username);

        /// <summary>
        /// Check if email is available
        /// </summary>
        Task<bool> IsEmailUniqueAsync(string email);
    }
} 