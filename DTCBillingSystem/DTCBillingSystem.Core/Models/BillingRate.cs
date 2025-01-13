using System;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents different types of billing rates in the system
    /// </summary>
    public class BillingRate : BaseEntity
    {
        /// <summary>
        /// Type of the rate (e.g., Electricity, AC, Generator)
        /// </summary>
        public string RateType { get; set; }

        /// <summary>
        /// The actual rate amount
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// When this rate becomes effective
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// When this rate expires (null if current)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Description or notes about the rate
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unit of measurement (e.g., per kWh, per hour)
        /// </summary>
        public string Unit { get; set; }

        public BillingRate()
        {
            IsActive = true;
            EffectiveDate = DateTime.UtcNow;
        }
    }
} 