using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class DailyCollectionReport
    {
        public DateTime Date { get; set; }
        public decimal TotalCollections { get; set; }
        public Dictionary<PaymentMethod, decimal> PaymentsByMethod { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public decimal PendingCollections { get; set; }
        public decimal OverdueCollections { get; set; }
    }

    public class MonthlyBillingReport
    {
        public DateTime Month { get; set; }
        public decimal TotalBilledAmount { get; set; }
        public int TotalBills { get; set; }
        public Dictionary<BillStatus, int> BillsByStatus { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CollectionRate { get; set; }
        public List<CustomerBillingSummary> TopCustomers { get; set; }
    }

    public class OutstandingPaymentsReport
    {
        public DateTime GeneratedAt { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public int TotalOutstandingBills { get; set; }
        public Dictionary<string, decimal> OutstandingByAgeGroup { get; set; }
        public List<CustomerOutstandingBills> CustomerDetails { get; set; }
    }

    public class CustomerBillingSummary
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int TotalBills { get; set; }
        public DateTime LastBillingDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    public class CustomerOutstandingBills
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int NumberOfBills { get; set; }
        public DateTime OldestBillDate { get; set; }
        public List<BillSummary> Bills { get; set; }
    }

    public class BillSummary
    {
        public int BillId { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int DaysOverdue { get; set; }
        public BillStatus Status { get; set; }
    }
} 