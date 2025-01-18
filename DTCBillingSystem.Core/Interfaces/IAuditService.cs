using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActivityAsync(string entityType, string action, int userId, string details);
    }
} 