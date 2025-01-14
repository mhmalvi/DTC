using System;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents different types of billing rates in the system
    /// </summary>
    public class BillingRate : BaseEntity
    {
        /// <summary>
        /// The rate code
        /// </summary>
        public string RateCode { get; set; }

        /// <summary>
        /// Description or notes about the rate
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The base rate amount
        /// </summary>
        public decimal BaseRate { get; set; }

        /// <summary>
        /// The tax rate amount
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When this rate becomes effective
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// When this rate expires (null if current)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        public BillingRate()
        {
            IsActive = true;
            EffectiveFrom = DateTime.Now;
        }
    }
} 