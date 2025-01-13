using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a shop owner/customer in the trade center
    /// </summary>
    public class Customer : BaseEntity
    {
        /// <summary>
        /// Full name of the customer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Shop number in the trade center
        /// </summary>
        public string ShopNo { get; set; }

        /// <summary>
        /// Floor number where the shop is located
        /// </summary>
        public string Floor { get; set; }

        /// <summary>
        /// Contact phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// When the customer registered their shop
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Whether the customer is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Email address for notifications
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Additional notes about the customer
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Navigation property for customer's bills
        /// </summary>
        public virtual ICollection<MonthlyBill> Bills { get; set; }

        public Customer()
        {
            Bills = new List<MonthlyBill>();
            IsActive = true;
            RegistrationDate = DateTime.UtcNow;
        }
    }
} 