using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class Customer : BaseEntity
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CustomerType Type { get; set; }
        public string MeterNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
} 