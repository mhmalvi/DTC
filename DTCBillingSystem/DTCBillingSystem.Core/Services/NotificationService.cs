using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public NotificationService(
            ILogger<NotificationService> logger,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<NotificationMessage> CreateNotificationAsync(NotificationMessage notification, int userId)
        {
            try
            {
                notification.Status = NotificationStatus.Pending;
                notification.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogCreateAsync(notification, userId);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task UpdateNotificationStatusAsync(int notificationId, string status, int userId)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    throw new ArgumentException("Notification not found", nameof(notificationId));
                }

                notification.Status = Enum.Parse<NotificationStatus>(status);
                notification.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogUpdateAsync(notification, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification status for notification {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    throw new ArgumentException("Notification not found", nameof(notificationId));
                }

                await _unitOfWork.Notifications.DeleteAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogDeleteAsync(notification, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                throw;
            }
        }
    }
} 