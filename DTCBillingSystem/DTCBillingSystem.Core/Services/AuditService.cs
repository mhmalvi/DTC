using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(string entityType, string entityId, string userId, AuditAction action, string? oldValues = null, string? newValues = null)
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action.ToString(),
                OldValues = oldValues,
                NewValues = newValues,
                Changes = GetChanges(oldValues, newValues),
                CreatedBy = userId
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string entityType, string entityId)
        {
            return await _unitOfWork.AuditLogs.GetByEntityAsync(entityType, entityId);
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId)
        {
            return await _unitOfWork.AuditLogs.GetByUserAsync(userId);
        }

        private string? GetChanges(string? oldValues, string? newValues)
        {
            if (string.IsNullOrEmpty(oldValues) || string.IsNullOrEmpty(newValues))
                return null;

            // TODO: Implement a more sophisticated change detection logic
            return $"Changed from {oldValues} to {newValues}";
        }
    }
} 