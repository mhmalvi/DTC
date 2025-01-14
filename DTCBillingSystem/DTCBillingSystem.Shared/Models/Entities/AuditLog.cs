using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public AuditAction Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string Notes { get; set; }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 