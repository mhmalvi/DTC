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
        /// Get print job status
        /// </summary>
        Task<PrintJobStatus> GetPrintJobStatusAsync(string jobId);

        /// <summary>
        /// Cancel print job
        /// </summary>
        Task CancelPrintJobAsync(string jobId);
    }

    public class PrintOptions
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; } = 1;
        public bool Duplex { get; set; }
        public PaperSize PaperSize { get; set; } = PaperSize.A4;
        public PrintQuality Quality { get; set; } = PrintQuality.Normal;
        public bool ColorPrint { get; set; }
        public string Watermark { get; set; }
        public Dictionary<string, object> CustomSettings { get; set; }
    }

    public class PrintResult
    {
        public bool Success { get; set; }
        public string JobId { get; set; }
        public DateTime PrintTime { get; set; }
        public string PrinterName { get; set; }
        public int PagesPrinted { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PrinterInfo
    {
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool IsOnline { get; set; }
        public bool SupportsColor { get; set; }
        public bool SupportsDuplex { get; set; }
        public List<PaperSize> SupportedPaperSizes { get; set; }
        public string Location { get; set; }
    }

    public class PrintJobStatus
    {
        public string JobId { get; set; }
        public string DocumentName { get; set; }
        public PrintJobState State { get; set; }
        public int PagesPrinted { get; set; }
        public int TotalPages { get; set; }
        public DateTime SubmissionTime { get; set; }
        public string StatusMessage { get; set; }
    }

    public enum PaperSize
    {
        A4,
        A5,
        Letter,
        Legal,
        Custom
    }

    public enum PrintQuality
    {
        Draft,
        Normal,
        High
    }

    public enum PrintJobState
    {
        Pending,
        Printing,
        Completed,
        Failed,
        Cancelled
    }
} 