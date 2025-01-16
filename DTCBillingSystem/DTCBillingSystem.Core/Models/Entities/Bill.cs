using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Bill : BaseEntity
    {
        public int CustomerId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaymentReference { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
} 