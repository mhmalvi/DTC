using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class PaymentRecord : BaseEntity
    {
        public string TransactionId { get; set; } = string.Empty;
        public int MonthlyBillId { get; set; }
        public int CustomerId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal LatePaymentCharges { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ReferenceNumber { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual MonthlyBill MonthlyBill { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
    }
} 