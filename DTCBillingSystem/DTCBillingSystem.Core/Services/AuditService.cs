using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AuditService(ILogger<AuditService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuditLog> LogCreateAsync<T>(T entity, int userId, string ipAddress = null) where T : BaseEntity
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = typeof(T).Name,
                    EntityId = entity.Id,
                    UserId = userId,
                    Action = AuditAction.Create,
                    NewValues = JsonSerializer.Serialize(entity),
                    Notes = $"Created from {ipAddress ?? "unknown IP"}",
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging create action for entity {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<AuditLog> LogUpdateAsync<T>(T oldEntity, T newEntity, int userId, string ipAddress = null) where T : BaseEntity
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = typeof(T).Name,
                    EntityId = newEntity.Id,
                    UserId = userId,
                    Action = AuditAction.Update,
                    OldValues = JsonSerializer.Serialize(oldEntity),
                    NewValues = JsonSerializer.Serialize(newEntity),
                    Notes = $"Updated from {ipAddress ?? "unknown IP"}",
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging update action for entity {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<AuditLog> LogDeleteAsync<T>(T entity, int userId, string ipAddress = null) where T : BaseEntity
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = typeof(T).Name,
                    EntityId = entity.Id,
                    UserId = userId,
                    Action = AuditAction.Delete,
                    OldValues = JsonSerializer.Serialize(entity),
                    Notes = $"Deleted from {ipAddress ?? "unknown IP"}",
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging delete action for entity {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<AuditLog> LogActionAsync(
            string entityType,
            int entityId,
            AuditAction action,
            int userId,
            string oldValues = null,
            string newValues = null,
            string notes = null,
            string ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = userId,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    Notes = notes ?? $"Action performed from {ipAddress ?? "unknown IP"}",
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging custom action for entity {EntityType}", entityType);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, int entityId)
        {
            try
            {
                return await _unitOfWork.AuditLogs.GetByEntityAsync(entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for entity {EntityType} with ID {EntityId}", entityType, entityId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.AuditLogs.GetByUserAsync(userId, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetActionAuditLogsAsync(AuditAction action, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.AuditLogs.GetByActionAsync(action, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for action {Action}", action);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _unitOfWork.AuditLogs.GetByDateRangeAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for date range");
                throw;
            }
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(
            int pageIndex,
            int pageSize,
            string entityType = null,
            int? entityId = null,
            AuditAction? action = null,
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.AuditLogs.GetPagedAsync(
                    pageIndex,
                    pageSize,
                    entityType,
                    entityId,
                    action,
                    userId,
                    startDate,
                    endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged audit logs");
                throw;
            }
        }

        public async Task<byte[]> ExportAuditLogsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var logs = await GetAuditLogsAsync(startDate, endDate);
                // TODO: Implement CSV export logic
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit logs");
                throw;
            }
        }

        private static int GetEntityId<T>(T entity) where T : BaseEntity
        {
            return entity.Id;
        }
    }
} 