using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MonthlyBill
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public required string BillNumber { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime BillingMonth { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PresentReading { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal ACPresentReading { get; set; }
        public decimal ACPreviousReading { get; set; }
        public decimal Consumption { get; set; }
        public decimal BlowerFanCharge { get; set; }
        public decimal GeneratorCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public BillStatus Status { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
        public virtual ICollection<PaymentRecord> Payments { get; set; } = new List<PaymentRecord>();
    }
} 