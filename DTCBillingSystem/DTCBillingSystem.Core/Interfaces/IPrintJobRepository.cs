using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
        Task<IEnumerable<PrintJob>> GetFailedJobsAsync();
        Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(PrintJobStatus status);
    }
} 