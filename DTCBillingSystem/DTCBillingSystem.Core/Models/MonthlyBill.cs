using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a monthly bill for a customer
    /// </summary>
    public class MonthlyBill : BaseEntity
    {
        /// <summary>
        /// Reference to the customer
        /// </summary>
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// The month this bill is for
        /// </summary>
        public DateTime BillingMonth { get; set; }

        /// <summary>
        /// Current electricity meter reading
        /// </summary>
        public decimal PresentReading { get; set; }

        /// <summary>
        /// Previous electricity meter reading
        /// </summary>
        public decimal PreviousReading { get; set; }

        /// <summary>
        /// Current AC meter reading
        /// </summary>
        public decimal ACPresentReading { get; set; }

        /// <summary>
        /// Previous AC meter reading
        /// </summary>
        public decimal ACPreviousReading { get; set; }

        /// <summary>
        /// Charge for blower fan usage
        /// </summary>
        public decimal BlowerFanCharge { get; set; }

        /// <summary>
        /// Charge for generator usage
        /// </summary>
        public decimal GeneratorCharge { get; set; }

        /// <summary>
        /// Service charge amount
        /// </summary>
        public decimal ServiceCharge { get; set; }

        /// <summary>
        /// When the bill is due
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Current status of the bill
        /// </summary>
        public BillStatus Status { get; set; }

        /// <summary>
        /// Total amount for the bill
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Any additional charges
        /// </summary>
        public decimal AdditionalCharges { get; set; }

        /// <summary>
        /// Notes about the bill
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Navigation property for payments
        /// </summary>
        public virtual ICollection<PaymentRecord> Payments { get; set; }

        public MonthlyBill()
        {
            Payments = new List<PaymentRecord>();
            Status = BillStatus.Pending;
            DueDate = DateTime.UtcNow.AddDays(30);
        }
    }

    /// <summary>
    /// Represents the status of a bill
    /// </summary>
    public enum BillStatus
    {
        Pending,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled,
        Disputed
    }
} 