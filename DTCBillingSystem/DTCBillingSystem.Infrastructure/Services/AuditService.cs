using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogActionAsync(string entityType, object? entityId, string action, string details)
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId?.ToString() ?? "0",
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
        }

        public async Task LogAsync(string entityType, string entityId, int userId, string action, string? details = null)
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Action = action,
                Details = details ?? string.Empty,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
        }
    }
} 