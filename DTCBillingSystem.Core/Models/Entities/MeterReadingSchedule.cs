using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MeterReadingSchedule : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime ReadingDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;

        // Navigation property
        public virtual Customer Customer { get; set; } = null!;
    }
} 