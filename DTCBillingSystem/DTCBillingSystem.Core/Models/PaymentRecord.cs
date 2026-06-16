using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class PaymentRecord
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid BillId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public string? ReceiptNumber { get; set; }
    }
} 