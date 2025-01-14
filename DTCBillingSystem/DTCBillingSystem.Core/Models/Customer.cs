using System;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a shop owner/customer in the trade center
    /// </summary>
    public class Customer : BaseEntity
    {
        /// <summary>
        /// Customer code
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// First name of the customer
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the customer
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Address of the customer
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Contact number of the customer
        /// </summary>
        public string ContactNumber { get; set; }

        /// <summary>
        /// Email address of the customer
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Type of customer (Residential, Commercial, etc.)
        /// </summary>
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// When the customer registered their shop
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Whether the customer is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Navigation property for customer's bills
        /// </summary>
        public virtual ICollection<MonthlyBill> Bills { get; set; }

        public Customer()
        {
            Bills = new HashSet<MonthlyBill>();
            RegistrationDate = DateTime.Now;
            IsActive = true;
            CustomerType = CustomerType.Residential; // Default to Residential
        }
    }
} 