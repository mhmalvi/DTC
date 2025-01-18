using System;
using DTCBillingSystem.Core.Models.Enums;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class MonthlyBill : BaseEntity
    {
        public int CustomerId { get; set; }
        public DateTime BillingDate { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Consumption { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
} 