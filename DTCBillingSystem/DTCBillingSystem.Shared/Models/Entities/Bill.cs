using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class Bill : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public string? BillNumber { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<PaymentRecord>? Payments { get; set; }
    }
} 