using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

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
        /// Schedule a notification
        /// </summary>
        Task ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledTime);
    }

    public class NotificationMessage
    {
        public int RecipientId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public NotificationChannel[] Channels { get; set; }
        public AlertPriority Priority { get; set; }
    }

    public class NotificationHistory
    {
        public int Id { get; set; }
        public int RecipientId { get; set; }
        public DateTime SentTime { get; set; }
        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool Successful { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class NotificationSettings
    {
        public bool EmailEnabled { get; set; }
        public bool SmsEnabled { get; set; }
        public bool WhatsAppEnabled { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public NotificationType[] EnabledNotificationTypes { get; set; }
        public Dictionary<NotificationType, NotificationChannel[]> ChannelPreferences { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }

    public enum NotificationType
    {
        BillGenerated,
        PaymentReceived,
        PaymentDue,
        PaymentOverdue,
        SystemAlert,
        AccountUpdate,
        MaintenanceNotice
    }

    public enum NotificationChannel
    {
        Email,
        SMS,
        WhatsApp,
        InApp
    }

    public enum AlertPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
} 