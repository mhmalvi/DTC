using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Customer : BaseEntity
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ShopNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public CustomerType CustomerType { get; set; }
        public string? Notes { get; set; }
        public string MeterNumber { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public CustomerStatus Status { get; set; }
        public DateTime RegistrationDate { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal SecurityDeposit { get; set; }

        public string Name => $"{FirstName} {LastName}".Trim();

        // Navigation properties
        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<MeterReadingSchedule> ReadingSchedules { get; set; } = new List<MeterReadingSchedule>();
        public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();
    }
} 