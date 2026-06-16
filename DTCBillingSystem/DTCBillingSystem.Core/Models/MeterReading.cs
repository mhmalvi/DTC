using System;

namespace DTCBillingSystem.Core.Models
{
    public class MeterReading : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal MainReading { get; set; }
        public decimal ACReading { get; set; }
        public decimal? BlowerFanReading { get; set; }
        public required string ReadBy { get; set; }
        public string? Notes { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public int? RecordedByUserId { get; set; }
        public int? LastModifiedByUserId { get; set; }

        // Navigation properties
        public required Customer Customer { get; set; }
        public User? RecordedByUser { get; set; }
        public User? LastModifiedByUser { get; set; }

        public MeterReading Clone()
        {
            return (MeterReading)this.MemberwiseClone();
        }
    }
} 