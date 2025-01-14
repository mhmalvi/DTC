using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

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
        Task<Dictionary<string, object>> GenerateCustomReportAsync(string reportType, Dictionary<string, object> parameters);
    }
} 