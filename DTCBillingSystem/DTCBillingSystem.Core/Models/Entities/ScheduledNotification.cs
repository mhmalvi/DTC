using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class ScheduledNotification : BaseEntity
    {
        public NotificationMessage Message { get; set; } = new();
        public DateTime ScheduledTime { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime? SentTime { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryTime { get; set; }
    }
} 