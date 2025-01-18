using System;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public required string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public int InvoiceId { get; set; }
        public virtual Invoice? Invoice { get; set; }
    }
} 