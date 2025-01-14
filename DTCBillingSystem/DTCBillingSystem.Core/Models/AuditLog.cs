using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents an audit log entry in the system
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// Type of entity being audited
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// ID of the entity being audited
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Type of action performed
        /// </summary>
        public AuditAction Action { get; set; }

        /// <summary>
        /// User who performed the action
        /// </summary>
        public int UserId { get; set; }
        public virtual User User { get; set; }

        /// <summary>
        /// When the action was performed
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Previous state of the entity (JSON)
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// New state of the entity (JSON)
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// Additional information about the change
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// IP address of the user
        /// </summary>
        public string IpAddress { get; set; }

        public AuditLog()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
} 