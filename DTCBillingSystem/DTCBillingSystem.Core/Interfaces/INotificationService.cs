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
        Task SendSystemAlertAsync(string message, AlertPriority priority);

        /// <summary>
        /// Send bulk notifications
        /// </summary>
        Task SendBulkNotificationsAsync(IEnumerable<NotificationMessage> messages);

        /// <summary>
        /// Get notification history for a customer
        /// </summary>
        Task<IEnumerable<NotificationHistory>> GetCustomerNotificationHistoryAsync(int customerId);

        /// <summary>
        /// Get notification settings for a customer
        /// </summary>
        Task<NotificationSettings> GetCustomerNotificationSettingsAsync(int customerId);

        /// <summary>
        /// Update notification settings for a customer
        /// </summary>
        Task UpdateCustomerNotificationSettingsAsync(int customerId, NotificationSettings settings);

        /// <summary>
        /// Schedule a notification for future delivery
        /// </summary>
        Task ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledTime);
    }
} 