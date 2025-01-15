using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
        Task<IEnumerable<PrintJob>> GetFailedJobsAsync();
        Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(PrintJobStatus status);
    }
} 