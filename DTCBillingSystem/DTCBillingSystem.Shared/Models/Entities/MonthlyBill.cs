using System;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class MonthlyBill : BaseEntity
    {
        public string BillNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime BillingDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal Consumption { get; set; }
        public BillStatus Status { get; set; }
        public string BillingPeriod { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual ICollection<PaymentRecord> Payments { get; set; }

        public MonthlyBill()
        {
            Payments = new HashSet<PaymentRecord>();
            BillingDate = DateTime.Now;
            DueDate = BillingDate.AddDays(30);
            Status = BillStatus.Pending;
            TaxAmount = 0;
            TotalAmount = 0;
        }
    }
} 