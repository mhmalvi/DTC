using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CustomerType Type { get; set; }
        public string ZoneCode { get; set; } = string.Empty;
        public string MeterNumber { get; set; } = string.Empty;
        public DateTime ConnectionDate { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal SecurityDeposit { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime LastBillingDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }
} 