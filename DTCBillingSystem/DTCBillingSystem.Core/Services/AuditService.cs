using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task LogCreateAsync<T>(T entity, int userId, string notes = null)
        {
            await LogActionAsync(
                typeof(T).Name,
                userId,
                AuditAction.Create,
                0,
                null,
                JsonSerializer.Serialize(entity),
                notes,
                null);
        }

        public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, int userId, string notes = null)
        {
            await LogActionAsync(
                typeof(T).Name,
                userId,
                AuditAction.Update,
                0,
                JsonSerializer.Serialize(oldEntity),
                JsonSerializer.Serialize(newEntity),
                notes,
                null);
        }

        public async Task LogDeleteAsync<T>(T entity, int userId, string notes = null)
        {
            await LogActionAsync(
                typeof(T).Name,
                userId,
                AuditAction.Delete,
                0,
                JsonSerializer.Serialize(entity),
                null,
                notes,
                null);
        }

        public async Task LogActionAsync(
            string action,
            int userId,
            AuditAction auditAction,
            int entityId,
            string entityType,
            string description,
            string oldValue = null,
            string newValue = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    UserId = userId,
                    AuditAction = auditAction,
                    EntityId = entityId,
                    EntityType = entityType,
                    Description = description,
                    OldValue = oldValue,
                    NewValue = newValue,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = _currentUserService.IpAddress
                };

                await _unitOfWork.AuditLogs.AddAsync(auditLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action {Action} for user {UserId}", action, userId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _unitOfWork.AuditLogs.GetAll()
                    .Where(log => log.UserId == userId);

                if (fromDate.HasValue)
                    query = query.Where(log => log.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(log => log.Timestamp <= toDate.Value);

                return await Task.FromResult(query.OrderByDescending(log => log.Timestamp).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetActionAuditLogsAsync(AuditAction action, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _unitOfWork.AuditLogs.GetAll()
                    .Where(log => log.AuditAction == action);

                if (fromDate.HasValue)
                    query = query.Where(log => log.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(log => log.Timestamp <= toDate.Value);

                return await Task.FromResult(query.OrderByDescending(log => log.Timestamp).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for action {Action}", action);
                throw;
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var query = _unitOfWork.AuditLogs.GetAll()
                    .Where(log => log.Timestamp >= fromDate && log.Timestamp <= toDate)
                    .OrderByDescending(log => log.Timestamp);

                return await Task.FromResult(query.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs for date range {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            int? userId = null,
            AuditAction? action = null,
            int? entityId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var query = _unitOfWork.AuditLogs.GetAll();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                    query = query.Where(log => log.Description.Contains(searchTerm));

                if (userId.HasValue)
                    query = query.Where(log => log.UserId == userId.Value);

                if (action.HasValue)
                    query = query.Where(log => log.AuditAction == action.Value);

                if (entityId.HasValue)
                    query = query.Where(log => log.EntityId == entityId.Value);

                if (fromDate.HasValue)
                    query = query.Where(log => log.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(log => log.Timestamp <= toDate.Value);

                var totalCount = query.Count();
                var logs = query
                    .OrderByDescending(log => log.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (logs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged audit logs");
                throw;
            }
        }

        public async Task<byte[]> ExportAuditLogsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var logs = await GetAuditLogsAsync(fromDate, toDate);
                var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit logs for date range {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }
    }

    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var param = first.Parameters[0];
            var body = Expression.AndAlso(first.Body, new ExpressionReplacer(second.Parameters[0], param).Visit(second.Body));
            return Expression.Lambda<Func<T, bool>>(body, param);
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