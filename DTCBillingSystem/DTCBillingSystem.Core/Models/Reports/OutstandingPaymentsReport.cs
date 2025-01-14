using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models.Reports
{
    public class OutstandingPaymentsReport
    {
        public DateTime AsOfDate { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public int TotalCustomersWithOutstanding { get; set; }
        public Dictionary<int, decimal> OutstandingByAgeingDays { get; set; } = new();
        public List<CustomerOutstanding> CustomerOutstandings { get; set; } = new();
        public decimal AverageOutstandingAmount { get; set; }
        public int OldestOutstandingDays { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public int TotalOverdueBills { get; set; }
    }

    public class CustomerOutstanding
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string MeterNumber { get; set; } = string.Empty;
        public decimal OutstandingAmount { get; set; }
        public int OldestBillDays { get; set; }
        public List<MonthlyBill> OutstandingBills { get; set; } = new();
        public decimal OverdueAmount { get; set; }
        public int OverdueBills { get; set; }
    }
} 