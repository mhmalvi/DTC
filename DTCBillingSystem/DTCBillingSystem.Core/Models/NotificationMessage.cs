using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationMessage : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int RecipientId { get; set; }
        public NotificationType Type { get; set; }
        public new DateTime CreatedAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }
} 