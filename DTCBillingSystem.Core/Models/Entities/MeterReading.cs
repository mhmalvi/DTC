using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MeterReading : BaseEntity
    {
        public int CustomerId { get; set; }
        public string MeterNumber { get; set; } = string.Empty;
        public decimal Reading { get; set; }
        public DateTime ReadingDate { get; set; }
        public string ReadBy { get; set; } = string.Empty;
        public ReadingSource Source { get; set; }
        public ReadingStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal? PreviousReading { get; set; }
        public decimal? Consumption { get; set; }
        public bool IsAnomalous { get; set; }
        public string ValidationNotes { get; set; } = string.Empty;

        public virtual Customer Customer { get; set; } = null!;
    }
} 