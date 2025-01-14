using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class MeterReading : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Consumption { get; set; }
        public string? Notes { get; set; }
        public bool IsProcessed { get; set; }
        public string ReadBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual Customer? Customer { get; set; }
    }
} 