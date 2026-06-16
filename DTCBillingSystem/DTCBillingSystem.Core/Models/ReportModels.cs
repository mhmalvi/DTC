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

    public class CustomerStatement
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StatementPeriodStart { get; set; }
        public DateTime StatementPeriodEnd { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalCharges { get; set; }
        public decimal TotalPayments { get; set; }
        public List<BillTransaction> Transactions { get; set; }
        public List<OverdueBill> OverdueBills { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class BillTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string ReferenceNumber { get; set; }
        public string TransactionType { get; set; }
    }

    public class OverdueBill
    {
        public int BillId { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public int DaysOverdue { get; set; }
        public decimal LateCharges { get; set; }
    }

    public class RevenueSummaryReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCollections { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public Dictionary<PaymentMethod, decimal> CollectionsByMethod { get; set; }
        public List<MonthlyRevenue> MonthlyBreakdown { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class MonthlyRevenue
    {
        public DateTime Month { get; set; }
        public decimal BilledAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal CollectionRate { get; set; }
        public int TotalBills { get; set; }
        public int PaidBills { get; set; }
        public int OverdueBills { get; set; }
    }
} 