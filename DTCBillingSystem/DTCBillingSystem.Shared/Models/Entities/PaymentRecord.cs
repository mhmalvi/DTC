using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class PaymentRecord : BaseEntity
    {
        public string PaymentNumber { get; set; }
        public int BillId { get; set; }
        public decimal PaymentAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string Notes { get; set; }
        public MonthlyBill Bill { get; set; }
    }
} 