using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? AffectedColumns { get; set; }
        public string? ClientIp { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
} 