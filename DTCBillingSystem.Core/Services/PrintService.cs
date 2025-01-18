using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public PrintService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<bool> QueuePrintJobAsync(string documentType, string content, int userId)
        {
            try
            {
                var notification = new Notification
                {
                    Title = $"Print Job - {documentType}",
                    Message = content,
                    Type = NotificationType.PrintJob,
                    IsRead = false,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
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
                    "Queue",
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
                {
                    return false;
                }

                notification.IsRead = true;
                notification.LastModifiedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Complete",
                    userId,
                    $"Completed print job {notification.Id}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Complete",
                    userId,
                    $"Failed to complete print job: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<IEnumerable<Notification>> GetPendingPrintJobsAsync()
        {
            return await _unitOfWork.Notifications.FindAsync(n => 
                n.Type == NotificationType.PrintJob && 
                !n.IsRead
            );
        }

        public async Task<bool> CancelPrintJobAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            if (notification.Type != NotificationType.PrintJob)
                return false;

            try
            {
                await _unitOfWork.Notifications.RemoveAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Cancel",
                    userId,
                    $"Cancelled print job {notification.Id}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "PrintJob",
                    "Cancel",
                    userId,
                    $"Failed to cancel print job {notification.Id}: {ex.Message}"
                );
                return false;
            }
        }
    }
}