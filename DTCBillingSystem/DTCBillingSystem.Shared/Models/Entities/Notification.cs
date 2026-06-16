using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
        public int Priority { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
} 