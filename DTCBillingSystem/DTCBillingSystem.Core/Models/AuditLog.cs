using System;
using System.ComponentModel.DataAnnotations;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class AuditLog : BaseEntity
    {
        [Required]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string OldValues { get; set; } = string.Empty;

        [Required]
        public string NewValues { get; set; } = string.Empty;

        [Required]
        public string AffectedColumns { get; set; } = string.Empty;

        [Required]
        public string IPAddress { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public AuditLog()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
} 