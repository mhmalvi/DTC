using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Reports
{
    public class MonthlyBillingReport
    {
        public DateTime Month { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBillsGenerated { get; set; }
        public decimal TotalBilledAmount { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public Dictionary<BillStatus, int> BillsByStatus { get; set; } = new();
        public Dictionary<BillStatus, decimal> AmountsByStatus { get; set; } = new();
        public List<MonthlyBill> Bills { get; set; } = new();
        public decimal AverageBillAmount { get; set; }
        public decimal MedianBillAmount { get; set; }
        public int TotalOverdueBills { get; set; }
        public decimal TotalOverdueAmount { get; set; }
    }
} 