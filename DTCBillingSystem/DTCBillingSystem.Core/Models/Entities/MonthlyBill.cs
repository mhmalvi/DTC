using System;
using DTCBillingSystem.Core.Models.Enums;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MonthlyBill : BaseEntity
    {
        public int CustomerId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillingMonth { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        // Meter readings
        public decimal PresentReading { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal ACPresentReading { get; set; }
        public decimal ACPreviousReading { get; set; }

        // Additional charges
        public decimal BlowerFanCharge { get; set; }
        public decimal GeneratorCharge { get; set; }
        public decimal ServiceCharge { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
    }
} 