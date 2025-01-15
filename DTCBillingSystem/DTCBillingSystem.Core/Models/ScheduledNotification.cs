using System;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a notification scheduled for future delivery
    /// </summary>
    public class ScheduledNotification
    {
        /// <summary>
        /// The notification message to be sent
        /// </summary>
        public NotificationMessage Message { get; set; } = new();

        /// <summary>
        /// When the notification should be sent
        /// </summary>
        public DateTime ScheduledTime { get; set; }

        /// <summary>
        /// Current status of the scheduled notification
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Number of retry attempts if sending fails
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Last error message if sending failed
        /// </summary>
        public string LastError { get; set; } = string.Empty;

        /// <summary>
        /// When the notification was actually sent
        /// </summary>
        public DateTime? SentAt { get; set; }
    }
} 