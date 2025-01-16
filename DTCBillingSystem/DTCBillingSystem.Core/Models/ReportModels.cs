using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Reports
{
    public class BillingSummary
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBillsGenerated { get; set; }
        public decimal TotalBillAmount { get; set; }
        public decimal TotalPaymentsReceived { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public List<BillStatusSummary> BillsByStatus { get; set; } = new();
        public List<PaymentMethodSummary> PaymentsByMethod { get; set; } = new();
    }

    public class PaymentMethodSummary
    {
        public PaymentMethod Method { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DailyCollectionReport
    {
        public DateTime Date { get; set; }
        public decimal TotalCollection { get; set; }
        public int NumberOfPayments { get; set; }
        public List<PaymentSummary> PaymentsByMethod { get; set; } = new();
        public List<PaymentDetail> Payments { get; set; } = new();
    }

    public class MonthlyBillingReport
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalBilled { get; set; }
        public int NumberOfBills { get; set; }
        public decimal AverageBillAmount { get; set; }
        public List<BillingSummaryByZone> BillingsByZone { get; set; } = new();
        public List<BillDetail> Bills { get; set; } = new();
    }

    public class OutstandingPaymentsReport
    {
        public DateTime GeneratedDate { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int NumberOfCustomers { get; set; }
        public List<CustomerOutstanding> CustomerOutstandings { get; set; } = new();
    }

    public class RevenueSummaryReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal CollectionRate { get; set; }
        public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
    }

    public class CustomerStatement
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<StatementTransaction> Transactions { get; set; } = new();
        public List<OverdueBill> OverdueBills { get; set; } = new();
    }

    public class PaymentSummary
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class PaymentDetail
    {
        public DateTime PaymentDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
    }

    public class BillingSummaryByZone
    {
        public string Zone { get; set; } = string.Empty;
        public int NumberOfCustomers { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal AverageBill { get; set; }
    }

    public class BillDetail
    {
        public string BillNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CustomerOutstanding
    {
        public string CustomerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal OutstandingAmount { get; set; }
        public int NumberOfUnpaidBills { get; set; }
        public DateTime OldestUnpaidBillDate { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Billed { get; set; }
        public decimal Collected { get; set; }
        public decimal CollectionRate { get; set; }
    }

    public class StatementTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public StatementTransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }

    public class OverdueBill
    {
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class BillStatusSummary
    {
        public BillStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 