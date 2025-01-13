using System;

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
        public virtual MonthlyBill Bill { get; set; }

        /// <summary>
        /// Amount paid
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// When the payment was made
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// How the payment was made
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Any late payment charges included
        /// </summary>
        public decimal LatePaymentCharges { get; set; }

        /// <summary>
        /// Reference number for the transaction
        /// </summary>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Notes about the payment
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Who received the payment
        /// </summary>
        public string ReceivedBy { get; set; }

        public PaymentRecord()
        {
            PaymentDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents different methods of payment
    /// </summary>
    public enum PaymentMethod
    {
        Cash,
        Check,
        BankTransfer,
        MobilePayment,
        Other
    }
} 