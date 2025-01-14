using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Reports
{
    public class RevenueSummaryReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CollectionRate { get; set; }
        public Dictionary<PaymentMethod, decimal> RevenueByPaymentMethod { get; set; } = new();
        public Dictionary<DateTime, decimal> DailyRevenue { get; set; } = new();
        public Dictionary<DateTime, decimal> MonthlyRevenue { get; set; } = new();
        public decimal AverageDailyRevenue { get; set; }
        public decimal AverageMonthlyRevenue { get; set; }
        public DateTime HighestRevenueDate { get; set; }
        public decimal HighestDailyRevenue { get; set; }
        public DateTime LowestRevenueDate { get; set; }
        public decimal LowestDailyRevenue { get; set; }
    }
} 