using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Payment : BaseEntity
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public BillStatus Status { get; set; }
        public bool IsVoid { get; set; }
        public string? VoidReason { get; set; }

        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
    }
} 