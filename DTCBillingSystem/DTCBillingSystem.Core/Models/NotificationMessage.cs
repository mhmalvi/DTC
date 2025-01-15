using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents an email notification message
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// Email recipient address
        /// </summary>
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Carbon copy recipients
        /// </summary>
        public List<string> CC { get; set; } = new();

        /// <summary>
        /// Blind carbon copy recipients
        /// </summary>
        public List<string> BCC { get; set; } = new();

        /// <summary>
        /// Email subject line
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Email body content
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Whether the body contains HTML
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// Recipient ID
        /// </summary>
        public int RecipientId { get; set; }

        /// <summary>
        /// Recipient email address
        /// </summary>
        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>
        /// Recipient phone number
        /// </summary>
        public string RecipientPhone { get; set; } = string.Empty;

        /// <summary>
        /// Notification type
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Scheduled time for the notification
        /// </summary>
        public DateTime? ScheduledTime { get; set; }

        /// <summary>
        /// Optional attachments (file paths)
        /// </summary>
        public List<string> Attachments { get; set; } = new();
    }
} 