using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class PaymentRecord : BaseEntity
    {
        public int CustomerId { get; set; }
        public int MonthlyBillId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime PaymentDate { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public MonthlyBill MonthlyBill { get; set; } = null!;
    }
} 