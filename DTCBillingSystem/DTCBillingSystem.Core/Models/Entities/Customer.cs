using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string? ShopNo { get; set; }
        public string? Notes { get; set; }
        public decimal Balance { get; set; }
        public CustomerStatus Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public string? Floor { get; set; }
        public string? ZoneCode { get; set; }
        public CustomerType CustomerType { get; set; }
        public DateTime RegistrationDate { get; set; }
        public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();
    }
} 