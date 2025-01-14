using System;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class NotificationMessage : BaseEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecipientId { get; set; }
        public string SenderId { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType NotificationType { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
    }
} 