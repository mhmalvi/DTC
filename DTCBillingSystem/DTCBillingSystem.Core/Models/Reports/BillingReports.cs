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

    public class BillStatusSummary
    {
        public BillStatus Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentMethodSummary
    {
        public PaymentMethod Method { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerStatement
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<StatementTransaction> Transactions { get; set; } = new();
        public List<OverdueBill> OverdueBills { get; set; } = new();
    }

    public class StatementTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
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
} 