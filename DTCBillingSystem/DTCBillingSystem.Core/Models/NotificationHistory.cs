using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationHistory : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int CustomerId { get; set; }
        public string Channel { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? ReadDate { get; set; }
        public Customer Customer { get; set; }
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
    }
} 