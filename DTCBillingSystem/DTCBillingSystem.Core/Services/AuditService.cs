using System.Text.Json;
using Microsoft.Extensions.Logging;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(
            IUnitOfWork unitOfWork,
            ILogger<AuditService> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task LogActionAsync(
            string entityType,
            int entityId,
            AuditAction action,
            object oldValues,
            object newValues)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    UserId = _currentUserService.UserId,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUserService.IpAddress
                };

                // Serialize old values if provided
                if (oldValues != null)
                {
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }

                // Serialize new values if provided
                if (newValues != null)
                {
                    // If newValues is a string, use it directly, otherwise serialize
                    auditLog.NewValues = newValues is string ? newValues.ToString() 
                        : JsonSerializer.Serialize(newValues, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                }

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Audit log created for {EntityType} {EntityId}: {Action}",
                    entityType, entityId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating audit log for {EntityType} {EntityId}: {Action}",
                    entityType, entityId, action);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, int entityId)
        {
            try
            {
                var logs = await _unitOfWork.AuditLogs.FindAsync(l =>
                    l.EntityType == entityType && l.EntityId == entityId);

                return logs.OrderByDescending(l => l.Timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving audit logs for {EntityType} {EntityId}",
                    entityType, entityId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(int userId)
        {
            try
            {
                var logs = await _unitOfWork.AuditLogs.FindAsync(l => l.UserId == userId);
                return logs.OrderByDescending(l => l.Timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(
            int pageIndex,
            int pageSize,
            string entityType = null,
            AuditAction? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? userId = null)
        {
            try
            {
                // Build the predicate based on filter parameters
                Expression<Func<AuditLog, bool>> predicate = l => true;

                if (!string.IsNullOrEmpty(entityType))
                {
                    predicate = predicate.And(l => l.EntityType == entityType);
                }

                if (action.HasValue)
                {
                    predicate = predicate.And(l => l.Action == action.Value);
                }

                if (fromDate.HasValue)
                {
                    predicate = predicate.And(l => l.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate.And(l => l.Timestamp <= toDate.Value);
                }

                if (userId.HasValue)
                {
                    predicate = predicate.And(l => l.UserId == userId.Value);
                }

                return await _unitOfWork.AuditLogs.GetPagedAsync(pageIndex, pageSize, predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged audit logs");
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetRecentActivityAsync(int count = 50)
        {
            try
            {
                var logs = await _unitOfWork.AuditLogs.FindAsync(l => true);
                return logs.OrderByDescending(l => l.Timestamp).Take(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity logs");
                throw;
            }
        }

        public async Task CleanupOldLogsAsync(int daysToKeep)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldLogs = await _unitOfWork.AuditLogs.FindAsync(l => l.Timestamp < cutoffDate);

                foreach (var log in oldLogs)
                {
                    await _unitOfWork.AuditLogs.DeleteAsync(log);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up audit logs older than {DaysToKeep} days",
                    daysToKeep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old audit logs");
                throw;
            }
        }
    }

    // Extension method for combining predicates
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var firstBody = first.Body.Replace(first.Parameters[0], parameter);
            var secondBody = second.Body.Replace(second.Parameters[0], parameter);
            var body = Expression.AndAlso(firstBody, secondBody);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
        {
            return new ExpressionReplacer(searchEx, replaceEx).Visit(expression);
        }
    }

    internal class ExpressionReplacer : ExpressionVisitor
    {
        private readonly Expression _from, _to;

        public ExpressionReplacer(Expression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        public override Expression Visit(Expression node)
        {
            return node == _from ? _to : base.Visit(node);
        }
    }
} 