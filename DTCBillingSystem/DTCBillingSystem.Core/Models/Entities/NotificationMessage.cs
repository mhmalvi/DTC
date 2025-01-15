using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class NotificationMessage : BaseEntity
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string? CC { get; set; }
        public string? BCC { get; set; }
        public bool IsHtml { get; set; }
        public NotificationStatus Status { get; set; }
        public new DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryTime { get; set; }
        public NotificationType Type { get; set; }
        public int? CustomerId { get; set; }

        // Navigation property
        public virtual Customer? Customer { get; set; }
    }
} 