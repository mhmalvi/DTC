using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    /// <summary>
    /// Represents billing rates for different customer types
    /// </summary>
    public class BillingRate : BaseEntity
    {
        /// <summary>
        /// Type of customer this rate applies to
        /// </summary>
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// Base rate per unit
        /// </summary>
        public decimal BaseRate { get; set; }

        /// <summary>
        /// Additional rate per unit after threshold
        /// </summary>
        public decimal ExcessRate { get; set; }

        /// <summary>
        /// Units threshold before excess rate applies
        /// </summary>
        public decimal Threshold { get; set; }

        /// <summary>
        /// Fixed charges applied regardless of consumption
        /// </summary>
        public decimal FixedCharges { get; set; }

        /// <summary>
        /// Date from which this rate is effective
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Date until which this rate is effective (null means indefinite)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Any additional notes about this rate
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
} 