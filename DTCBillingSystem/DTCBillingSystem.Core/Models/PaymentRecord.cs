using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class PaymentRecord : BaseEntity
    {
        public int BillId { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public PaymentStatus Status { get; set; }
        public string Notes { get; set; }

        // Navigation property
        public virtual MonthlyBill Bill { get; set; }

        public PaymentRecord()
        {
            PaymentDate = DateTime.Now;
            Status = PaymentStatus.Completed;
        }
    }
} 