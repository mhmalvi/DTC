using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationHistory : BaseEntity
    {
        public Guid NotificationMessageId { get; set; }
        public DateTime AttemptDate { get; set; }
        public NotificationStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public int AttemptNumber { get; set; }

        // Navigation properties
        public virtual NotificationMessage? NotificationMessage { get; set; }
    }
} 