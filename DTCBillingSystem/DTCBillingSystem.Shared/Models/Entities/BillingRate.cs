using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BillingRate : BaseEntity
    {
        public decimal Rate { get; set; }
        public decimal TaxRate { get; set; }
        public decimal LatePaymentRate { get; set; }
        public CustomerType CustomerType { get; set; }
        public bool IsActive { get; set; }
    }
} 