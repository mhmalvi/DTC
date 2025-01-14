using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for audit logging operations
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Log an entity creation
        /// </summary>
        Task<AuditLog> LogCreateAsync<T>(T entity, int userId, string ipAddress = null) where T : BaseEntity;

        /// <summary>
        /// Log an entity update
        /// </summary>
        Task<AuditLog> LogUpdateAsync<T>(T oldEntity, T newEntity, int userId, string ipAddress = null) where T : BaseEntity;

        /// <summary>
        /// Log an entity deletion
        /// </summary>
        Task<AuditLog> LogDeleteAsync<T>(T entity, int userId, string ipAddress = null) where T : BaseEntity;

        /// <summary>
        /// Log a custom action
        /// </summary>
        Task<AuditLog> LogActionAsync(
            string entityType,
            int entityId,
            AuditAction action,
            int userId,
            string oldValues = null,
            string newValues = null,
            string notes = null,
            string ipAddress = null);

        /// <summary>
        /// Get audit logs for an entity
        /// </summary>
        Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, int entityId);

        /// <summary>
        /// Get audit logs by user
        /// </summary>
        Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get audit logs by action type
        /// </summary>
        Task<IEnumerable<AuditLog>> GetActionAuditLogsAsync(AuditAction action, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get audit logs within a date range
        /// </summary>
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get audit logs with pagination and filtering
        /// </summary>
        Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(
            int pageIndex,
            int pageSize,
            string entityType = null,
            int? entityId = null,
            AuditAction? action = null,
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Export audit logs to CSV
        /// </summary>
        Task<byte[]> ExportAuditLogsAsync(DateTime startDate, DateTime endDate);
    }
} 