using System;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Base class for all domain entities
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// When the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Who created the entity
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// When the entity was last modified
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Who last modified the entity
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// Indicates whether the entity is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            IsDeleted = false;
        }
    }
} 