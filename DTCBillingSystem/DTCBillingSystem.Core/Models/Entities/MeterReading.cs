using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MeterReading : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Reading { get; set; }
        public bool IsEstimated { get; set; }
        public string? Notes { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerificationDate { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
} 