using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class NotificationHistory : BaseEntity
    {
        public int CustomerId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime AttemptDate { get; set; }
        public int AttemptNumber { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
} 