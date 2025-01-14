using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class NotificationMessage : BaseEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string RecipientId { get; set; }
        public string SenderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public NotificationStatus Status { get; set; }
        public string NotificationType { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
        public bool IsRead { get; set; }
        public int Priority { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
} 