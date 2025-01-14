using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class MonthlyBill : BaseEntity
    {
        public string BillNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Consumption { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public BillStatus Status { get; set; }
        public string BillingPeriod { get; set; } = string.Empty;
        public string? Notes { get; set; }
        
        public Customer Customer { get; set; } = null!;
    }
} 