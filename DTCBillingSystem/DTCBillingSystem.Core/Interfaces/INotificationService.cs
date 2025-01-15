using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for notification operations
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send bill generation notification
        /// </summary>
        Task SendBillGeneratedNotificationAsync(int billId);

        /// <summary>
        /// Send payment received notification
        /// </summary>
        Task SendPaymentReceivedNotificationAsync(int paymentId);

        /// <summary>
        /// Send payment due reminder
        /// </summary>
        Task SendPaymentDueReminderAsync(int billId);

        /// <summary>
        /// Send overdue payment notification
        /// </summary>
        Task SendOverduePaymentNotificationAsync(int billId);

        /// <summary>
        /// Send system alert to administrators
        /// </summary>
        Task SendSystemAlertAsync(string message, NotificationType type);

        /// <summary>
        /// Send bulk notifications
        /// </summary>
        Task SendBulkNotificationsAsync(IEnumerable<NotificationMessage> messages);

        /// <summary>
        /// Get unread notifications for a user
        /// </summary>
        Task<IEnumerable<NotificationMessage>> GetUserNotificationsAsync(int userId);

        /// <summary>
        /// Get notification settings for a user
        /// </summary>
        Task<UserNotificationPreferences> GetUserNotificationSettingsAsync(int userId);

        /// <summary>
        /// Update notification settings for a user
        /// </summary>
        Task UpdateUserNotificationSettingsAsync(int userId, UserNotificationPreferences settings);

        /// <summary>
        /// Schedule a notification for future delivery
        /// </summary>
        Task ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledTime);
    }
} 