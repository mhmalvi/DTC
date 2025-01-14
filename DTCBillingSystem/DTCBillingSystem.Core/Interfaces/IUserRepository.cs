using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync(UserRole? role = null);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
    }
} 