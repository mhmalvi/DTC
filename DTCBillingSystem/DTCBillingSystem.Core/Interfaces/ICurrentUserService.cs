using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Models.Entities.User? CurrentUser { get; }
        void SetCurrentUser(Models.Entities.User? user);
        Task<Models.Entities.User?> GetCurrentUserAsync();
        void ClearCurrentUser();
    }
} 