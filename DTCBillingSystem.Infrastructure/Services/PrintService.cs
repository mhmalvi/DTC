using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class PrintService : IPrintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public PrintService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<bool> QueuePrintJobAsync(string documentType, string content, int userId)
        {
            try
            {
                var notification = new Notification
                {
                    Type = NotificationType.PrintJob,
                    Title = $"Print Job - {documentType}",
                    Message = content,
                    Status = NotificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    LastModifiedBy = userId
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Queue",
                    userId,
                    $"Queued print job for {documentType}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Error",
                    userId,
                    $"Failed to queue print job: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> CompletePrintJobAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notification == null || notification.Type != NotificationType.PrintJob)
                    return false;

                notification.Status = NotificationStatus.Completed;
                notification.LastModifiedAt = DateTime.UtcNow;
                notification.LastModifiedBy = userId;

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Complete",
                    userId,
                    $"Completed print job {notificationId}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Error",
                    userId,
                    $"Failed to complete print job {notificationId}: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<IEnumerable<Notification>> GetPendingPrintJobsAsync()
        {
            return await _unitOfWork.Notifications.FindAsync(n => 
                n.Type == NotificationType.PrintJob && 
                n.Status == NotificationStatus.Pending);
        }

        public async Task<bool> CancelPrintJobAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notification == null || notification.Type != NotificationType.PrintJob)
                    return false;

                notification.Status = NotificationStatus.Cancelled;
                notification.LastModifiedAt = DateTime.UtcNow;
                notification.LastModifiedBy = userId;

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Cancel",
                    userId,
                    $"Cancelled print job {notificationId}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Error",
                    userId,
                    $"Failed to cancel print job {notificationId}: {ex.Message}"
                );
                return false;
            }
        }
    }
} 