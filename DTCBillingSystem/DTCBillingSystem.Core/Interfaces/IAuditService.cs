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
        Task<AuditLog> LogCreateAsync<T>(T entity, int userId, string ipAddress = "") where T : BaseEntity;

        /// <summary>
        /// Log an entity update
        /// </summary>
        Task<AuditLog> LogUpdateAsync<T>(T oldEntity, T newEntity, int userId, string ipAddress = "") where T : BaseEntity;

        /// <summary>
        /// Log an entity deletion
        /// </summary>
        Task<AuditLog> LogDeleteAsync<T>(T entity, int userId, string ipAddress = "") where T : BaseEntity;

        /// <summary>
        /// Log a custom action
        /// </summary>
        Task<AuditLog> LogActionAsync(
            string entityName,
            int entityId,
            AuditAction action,
            int userId,
            string oldValues = "",
            string newValues = "",
            string notes = "",
            string ipAddress = "");

        /// <summary>
        /// Get audit logs for an entity
        /// </summary>
        Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityName, int entityId);

        /// <summary>
        /// Get audit logs by user
        /// </summary>
        Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(int userId);

        /// <summary>
        /// Get audit logs by action type
        /// </summary>
        Task<IEnumerable<AuditLog>> GetActionAuditLogsAsync(AuditAction action);

        /// <summary>
        /// Get audit logs within a date range
        /// </summary>
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate);
    }
} 