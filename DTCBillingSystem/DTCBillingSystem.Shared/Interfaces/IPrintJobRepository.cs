using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IPrintJobRepository : IRepository<PrintJob>
    {
        Task<IEnumerable<PrintJob>> GetPendingJobsAsync();
        Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(string status);
        Task<IEnumerable<PrintJob>> GetJobsByUserAsync(Guid userId);
        Task UpdateJobStatusAsync(Guid jobId, string status, string? errorMessage = null);
    }
} 