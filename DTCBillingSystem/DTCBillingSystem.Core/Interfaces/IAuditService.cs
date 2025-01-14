using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string entityType, string entityId, string userId, AuditAction action, string? oldValues = null, string? newValues = null);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityType, string entityId);
        Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId);
    }
} 