using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationHistory : BaseEntity
    {
        public string NotificationId { get; set; }
        public DateTime SentDate { get; set; }
        public NotificationStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryDate { get; set; }
    }
} 