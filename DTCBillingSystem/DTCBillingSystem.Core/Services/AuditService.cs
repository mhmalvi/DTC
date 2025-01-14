using System;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditService> _logger;

        public AuditService(IUnitOfWork unitOfWork, ILogger<AuditService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task LogCreateAsync<T>(T entity, Guid userId, string? notes = null) where T : class
        {
            var auditLog = new AuditLog
            {
                EntityType = typeof(T).Name,
                EntityId = GetEntityId(entity),
                Action = AuditAction.Create.ToString(),
                OldValues = null,
                NewValues = JsonSerializer.Serialize(entity),
                AffectedColumns = null,
                UserId = userId,
                ClientIp = "127.0.0.1", // TODO: Get actual IP address
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogUpdateAsync<T>(T entity, Guid userId, string? notes = null) where T : class
        {
            var auditLog = new AuditLog
            {
                EntityType = typeof(T).Name,
                EntityId = GetEntityId(entity),
                Action = AuditAction.Update.ToString(),
                OldValues = null, // TODO: Get old values from tracking
                NewValues = JsonSerializer.Serialize(entity),
                AffectedColumns = null,
                UserId = userId,
                ClientIp = "127.0.0.1", // TODO: Get actual IP address
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogDeleteAsync<T>(T entity, Guid userId, string? notes = null) where T : class
        {
            var auditLog = new AuditLog
            {
                EntityType = typeof(T).Name,
                EntityId = GetEntityId(entity),
                Action = AuditAction.Delete.ToString(),
                OldValues = JsonSerializer.Serialize(entity),
                NewValues = null,
                AffectedColumns = null,
                UserId = userId,
                ClientIp = "127.0.0.1", // TODO: Get actual IP address
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }

        private Guid GetEntityId<T>(T entity) where T : class
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");
            }

            var id = idProperty.GetValue(entity);
            if (id == null)
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} has a null Id");
            }

            if (id is Guid guidId)
            {
                return guidId;
            }
            else if (id is int intId)
            {
                // Convert int to Guid using a deterministic method
                return new Guid($"00000000-0000-0000-0000-{intId:D12}");
            }
            else
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} Id type {id.GetType().Name} is not supported");
            }
        }
    }
} 