using System;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a summary of billing operations for a given period
    /// </summary>
    public class BillingSummary
    {
        /// <summary>
        /// Start date of the summary period
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the summary period
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total number of bills generated
        /// </summary>
        public int TotalBills { get; set; }

        /// <summary>
        /// Total billed amount
        /// </summary>
        public decimal TotalBilledAmount { get; set; }

        /// <summary>
        /// Total amount collected
        /// </summary>
        public decimal TotalCollectedAmount { get; set; }

        /// <summary>
        /// Total outstanding amount
        /// </summary>
        public decimal TotalOutstandingAmount { get; set; }

        /// <summary>
        /// Number of overdue bills
        /// </summary>
        public int OverdueBillsCount { get; set; }

        /// <summary>
        /// Total amount of overdue bills
        /// </summary>
        public decimal OverdueAmount { get; set; }

        /// <summary>
        /// Collection efficiency percentage
        /// </summary>
        public decimal CollectionEfficiencyPercentage { get; set; }

        /// <summary>
        /// List of payment summaries by payment method
        /// </summary>
        public List<PaymentMethodSummary> PaymentMethodSummaries { get; set; }

        /// <summary>
        /// List of daily collection totals
        /// </summary>
        public List<DailyCollectionTotal> DailyCollectionTotals { get; set; }
    }

    /// <summary>
    /// Summary of payments by payment method
    /// </summary>
    public class PaymentMethodSummary
    {
        public PaymentMethod Method { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Daily collection total
    /// </summary>
    public class DailyCollectionTotal
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 