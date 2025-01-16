using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Reports;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate);
        Task<CustomerStatement> GetCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate);
        Task<DailyCollectionReport> GetDailyCollectionReportAsync(DateTime date);
        Task<MonthlyBillingReport> GetMonthlyBillingReportAsync(int month, int year);
        Task<OutstandingPaymentsReport> GetOutstandingPaymentsReportAsync();
        Task<RevenueSummaryReport> GetRevenueSummaryReportAsync(DateTime startDate, DateTime endDate);
    }
} 