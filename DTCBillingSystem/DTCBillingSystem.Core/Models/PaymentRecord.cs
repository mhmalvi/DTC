using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a payment made against a bill
    /// </summary>
    public class PaymentRecord : BaseEntity
    {
        /// <summary>
        /// Reference to the bill this payment is for
        /// </summary>
        public int BillId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod Method { get; set; }
        public string TransactionId { get; set; }
        public string PaymentDetails { get; set; }
        public string ReceivedBy { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }

        /// <summary>
        /// Navigation properties
        /// </summary>
        public virtual MonthlyBill Bill { get; set; }

        public PaymentRecord()
        {
            PaymentDate = DateTime.UtcNow;
        }
    }
} 