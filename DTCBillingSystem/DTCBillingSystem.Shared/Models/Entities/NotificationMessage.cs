using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationMessage : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 