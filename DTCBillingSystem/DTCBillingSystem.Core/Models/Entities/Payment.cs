using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Payment : BaseEntity
    {
        public int CustomerId { get; set; }
        public int BillId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string? CancelledReason { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelledBy { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Bill Bill { get; set; } = null!;
    }
} 