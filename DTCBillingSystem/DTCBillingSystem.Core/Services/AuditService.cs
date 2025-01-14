using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using System.Text.Json;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogCreateAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity
        {
            var auditLog = new AuditLog
            {
                EntityName = typeof(T).Name,
                EntityId = entity.Id.ToString(),
                Action = AuditAction.Create,
                NewValues = JsonSerializer.Serialize(entity),
                OldValues = string.Empty,
                Notes = notes ?? $"Created {typeof(T).Name}",
                UserId = userId.ToString(),
                IpAddress = "127.0.0.1" // Placeholder
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogUpdateAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity
        {
            var auditLog = new AuditLog
            {
                EntityName = typeof(T).Name,
                EntityId = entity.Id.ToString(),
                Action = AuditAction.Update,
                NewValues = JsonSerializer.Serialize(entity),
                OldValues = string.Empty, // Should retrieve old values from DB before update
                Notes = notes ?? $"Updated {typeof(T).Name}",
                UserId = userId.ToString(),
                IpAddress = "127.0.0.1" // Placeholder
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogDeleteAsync<T>(T entity, int userId, string notes = null) where T : BaseEntity
        {
            var auditLog = new AuditLog
            {
                EntityName = typeof(T).Name,
                EntityId = entity.Id.ToString(),
                Action = AuditAction.Delete,
                NewValues = string.Empty,
                OldValues = JsonSerializer.Serialize(entity),
                Notes = notes ?? $"Deleted {typeof(T).Name}",
                UserId = userId.ToString(),
                IpAddress = "127.0.0.1" // Placeholder
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogActionAsync(string entityType, int entityId, string action, int userId, string oldValues = null, string newValues = null, string notes = null)
        {
            var auditLog = new AuditLog
            {
                EntityName = entityType,
                EntityId = entityId.ToString(),
                Action = AuditAction.Other,
                NewValues = newValues ?? string.Empty,
                OldValues = oldValues ?? string.Empty,
                Notes = notes ?? $"Custom action on {entityType}",
                UserId = userId.ToString(),
                IpAddress = "127.0.0.1" // Placeholder
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
    }
} 