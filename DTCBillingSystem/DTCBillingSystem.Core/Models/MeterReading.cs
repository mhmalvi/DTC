using System;

namespace DTCBillingSystem.Core.Models
{
    public class MeterReading : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal MainReading { get; set; }
        public decimal ACReading { get; set; }
        public string ReadBy { get; set; }
        public string Notes { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerifiedBy { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
    }
} 