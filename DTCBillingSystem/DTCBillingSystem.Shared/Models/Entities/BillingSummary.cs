using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BillingSummary
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBills { get; set; }
        public decimal TotalBilledAmount { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public int OverdueBillsCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CollectionEfficiencyPercentage { get; set; }
        public List<PaymentMethodSummary> PaymentMethodSummaries { get; set; } = new();
        public List<DailyCollectionTotal> DailyCollectionTotals { get; set; } = new();
    }

    public class PaymentMethodSummary
    {
        public string Method { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DailyCollectionTotal
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 