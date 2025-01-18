using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string AccountNumber { get; set; }
        public required string MeterNumber { get; set; }
        public required string ContactNumber { get; set; }
        public string? Email { get; set; }
        public required string ZoneCode { get; set; }
        public required string ShopNo { get; set; }
        public string? Floor { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public CustomerType CustomerType { get; set; }
        public CustomerStatus Status { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ConnectionDate { get; set; }
        public DateTime? LastBillingDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();
        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
    }
} 