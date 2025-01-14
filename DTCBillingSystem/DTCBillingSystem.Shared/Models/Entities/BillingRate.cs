using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BillingRate : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CustomerType CustomerType { get; set; }
        public decimal RatePerUnit { get; set; }
        public decimal LatePaymentRate { get; set; } // Percentage
        public decimal MaintenanceFee { get; set; }
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
} 