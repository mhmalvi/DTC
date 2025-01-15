using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private const int SYSTEM_USER_ID = 1; // Default system user ID

        public AuditService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task LogActionAsync(string entityType, object? entityId, string action, string details)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId?.ToString() ?? "N/A",
                Action = action,
                Details = details,
                UserId = currentUser?.Id ?? SYSTEM_USER_ID,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
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
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
    }
} 