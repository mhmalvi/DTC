using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string entityType, object? entityId, string action, string details);
        Task LogAsync(string entityType, string entityId, int userId, string action, string? details = null);
    }
} 