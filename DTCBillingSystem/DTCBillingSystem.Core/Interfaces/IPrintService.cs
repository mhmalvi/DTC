using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for printing operations
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Print a single bill
        /// </summary>
        Task<PrintResult> PrintBillAsync(int billId, PrintOptions options = null);

        /// <summary>
        /// Print multiple bills
        /// </summary>
        Task<PrintResult> PrintBillsAsync(IEnumerable<int> billIds, PrintOptions options = null);

        /// <summary>
        /// Print customer statement
        /// </summary>
        Task<PrintResult> PrintCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate, PrintOptions options = null);

        /// <summary>
        /// Print daily collection report
        /// </summary>
        Task<PrintResult> PrintDailyCollectionReportAsync(DateTime date, PrintOptions options = null);

        /// <summary>
        /// Print monthly billing report
        /// </summary>
        Task<PrintResult> PrintMonthlyBillingReportAsync(DateTime month, PrintOptions options = null);

        /// <summary>
        /// Print receipt for payment
        /// </summary>
        Task<PrintResult> PrintReceiptAsync(int paymentId, PrintOptions options = null);

        /// <summary>
        /// Preview document before printing
        /// </summary>
        Task<byte[]> PreviewDocumentAsync(object document, PrintOptions options = null);

        /// <summary>
        /// Get list of available printers
        /// </summary>
        Task<IEnumerable<PrinterInfo>> GetAvailablePrintersAsync();

        /// <summary>
        /// Get status of a print job
        /// </summary>
        Task<PrintJobStatus> GetPrintJobStatusAsync(string jobId);

        /// <summary>
        /// Cancel a print job
        /// </summary>
        Task CancelPrintJobAsync(string jobId);
    }
} 