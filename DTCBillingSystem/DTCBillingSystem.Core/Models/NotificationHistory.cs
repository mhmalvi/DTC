using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationHistory : BaseEntity
    {
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string Channel { get; set; }
        public string DeliveryStatus { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
} 