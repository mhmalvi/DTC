using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime BillingMonth { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Consumption { get; set; }
        public BillStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PenaltyAmount { get; set; }
        public bool IsPenaltyApplied { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
} 