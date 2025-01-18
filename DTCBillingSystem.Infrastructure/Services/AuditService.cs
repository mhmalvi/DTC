using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task LogActivityAsync(string entityType, string action, int userId, string details)
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                Action = action,
                UserId = userId,
                Details = details,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                CreatedBy = int.Parse(_currentUserService.UserId),
                LastModifiedBy = int.Parse(_currentUserService.UserId)
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
    }
} 