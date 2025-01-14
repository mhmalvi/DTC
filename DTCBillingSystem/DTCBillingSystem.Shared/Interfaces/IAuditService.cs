using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IAuditService
    {
        Task LogCreateAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity;
        Task LogUpdateAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity;
        Task LogDeleteAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity;
        Task LogActionAsync(string entityType, int entityId, string action, int userId, string oldValues = null, string newValues = null, string notes = null);
    }
} 