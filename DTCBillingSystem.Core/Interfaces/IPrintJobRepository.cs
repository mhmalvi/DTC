using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPrintJobRepository : IBaseRepository<PrintJob>
    {
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
        Task<IEnumerable<PrintJob>> GetFailedJobsAsync();
        Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(PrintJobStatus status);
        Task RemoveAsync(PrintJob printJob);
    }
} 