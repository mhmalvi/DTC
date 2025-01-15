using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MonthlyBill : BaseEntity
    {
        public int CustomerId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public string BillingPeriod { get; set; } = string.Empty;
        public DateTime BillingMonth { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal PresentReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal ACPresentReading { get; set; }
        public decimal ACPreviousReading { get; set; }
        public decimal Consumption { get; set; }
        public decimal RatePerUnit { get; set; }
        public decimal BasicCharge { get; set; }
        public decimal BlowerFanCharge { get; set; }
        public decimal GeneratorCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public BillStatus Status { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
    }
} 