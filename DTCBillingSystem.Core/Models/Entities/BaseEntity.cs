using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public int LastModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
    }
} 