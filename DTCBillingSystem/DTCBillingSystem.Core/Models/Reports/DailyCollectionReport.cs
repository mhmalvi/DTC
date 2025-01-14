using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Reports
{
    public class DailyCollectionReport
    {
        public DateTime Date { get; set; }
        public decimal TotalCollected { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<PaymentMethod, decimal> CollectionsByMethod { get; set; } = new();
        public Dictionary<PaymentMethod, int> TransactionsByMethod { get; set; } = new();
        public List<PaymentRecord> Payments { get; set; } = new();
        public decimal CashCollections { get; set; }
        public decimal OnlineCollections { get; set; }
        public decimal CardCollections { get; set; }
        public decimal BankTransferCollections { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public PaymentMethod MostUsedPaymentMethod { get; set; }
    }
} 