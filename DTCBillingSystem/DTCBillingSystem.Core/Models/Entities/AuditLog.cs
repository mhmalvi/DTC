using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Changes { get; set; }
        public string? AdditionalInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public virtual User? User { get; set; }
    }
} 