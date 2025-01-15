using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<User?> GetCurrentUserAsync();
        Task LogoutAsync();
        bool IsAuthenticated { get; }
    }
} 