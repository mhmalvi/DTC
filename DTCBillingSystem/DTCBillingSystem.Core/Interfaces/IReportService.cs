using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for generating various reports
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generate daily collection report
        /// </summary>
        Task<DailyCollectionReport> GenerateDailyCollectionReportAsync(DateTime date);

        /// <summary>
        /// Generate monthly billing report
        /// </summary>
        Task<MonthlyBillingReport> GenerateMonthlyBillingReportAsync(DateTime month);

        /// <summary>
        /// Generate customer statement
        /// </summary>
        Task<CustomerStatement> GenerateCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate outstanding payments report
        /// </summary>
        Task<OutstandingPaymentsReport> GenerateOutstandingPaymentsReportAsync(DateTime? asOfDate = null);

        /// <summary>
        /// Generate revenue summary report
        /// </summary>
        Task<RevenueSummaryReport> GenerateRevenueSummaryReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Export report to PDF
        /// </summary>
        Task<byte[]> ExportToPdfAsync(object report);

        /// <summary>
        /// Export report to Excel
        /// </summary>
        Task<byte[]> ExportToExcelAsync(object report);

        /// <summary>
        /// Generate custom report
        /// </summary>
        Task<object> GenerateCustomReportAsync(string reportName, Dictionary<string, object> parameters);
    }

    public class DailyCollectionReport
    {
        public DateTime Date { get; set; }
        public decimal TotalCollected { get; set; }
        public int NumberOfPayments { get; set; }
        public List<PaymentRecord> Payments { get; set; }
        public Dictionary<PaymentMethod, decimal> CollectionsByMethod { get; set; }
    }

    public class MonthlyBillingReport
    {
        public DateTime Month { get; set; }
        public int TotalBillsGenerated { get; set; }
        public decimal TotalBilledAmount { get; set; }
        public decimal TotalCollectedAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public List<MonthlyBill> Bills { get; set; }
    }

    public class CustomerStatement
    {
        public Customer Customer { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<BillTransaction> Transactions { get; set; }
    }

    public class OutstandingPaymentsReport
    {
        public DateTime AsOfDate { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
        public int NumberOfOverdueBills { get; set; }
        public List<OverdueBill> OverdueBills { get; set; }
    }

    public class RevenueSummaryReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ElectricityRevenue { get; set; }
        public decimal ACRevenue { get; set; }
        public decimal GeneratorRevenue { get; set; }
        public decimal ServiceChargeRevenue { get; set; }
        public decimal LatePaymentCharges { get; set; }
        public List<MonthlyRevenue> MonthlyBreakdown { get; set; }
    }

    public class BillTransaction
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class OverdueBill
    {
        public Customer Customer { get; set; }
        public MonthlyBill Bill { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal LateCharges { get; set; }
    }

    public class MonthlyRevenue
    {
        public DateTime Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, decimal> RevenueByType { get; set; }
    }
} 