using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class PaymentRecord : BaseEntity
    {
        public int BillId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Reference { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        
        public MonthlyBill Bill { get; set; } = null!;
    }
} 