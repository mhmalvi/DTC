using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class MeterReading : BaseEntity
    {
        public string CustomerId { get; set; }
        public string MeterNumber { get; set; }
        public decimal Reading { get; set; }
        public DateTime ReadingDate { get; set; }
        public string ReadBy { get; set; }
        public bool IsEstimated { get; set; }
        public string Notes { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal Consumption { get; set; }
    }
} 