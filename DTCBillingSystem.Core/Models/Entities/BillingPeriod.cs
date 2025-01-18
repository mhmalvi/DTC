using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class BillingPeriod : BaseEntity
    {
        public required string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsClosed { get; set; }
    }
} 