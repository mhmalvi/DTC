using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BillingRate : BaseEntity
    {
        public string RateCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CustomerType CustomerType { get; set; }
        public decimal Rate { get; set; }
        public decimal TaxRate { get; set; }
        public decimal LatePaymentRate { get; set; }
        public bool IsActive { get; set; }
    }
} 