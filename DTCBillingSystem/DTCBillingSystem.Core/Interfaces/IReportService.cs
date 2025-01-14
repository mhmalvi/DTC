using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Reports;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IReportService
    {
        Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate);
        Task<CustomerStatement> GetCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate);
    }
} 