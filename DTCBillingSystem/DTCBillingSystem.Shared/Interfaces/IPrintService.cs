using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IPrintService
    {
        Task<PrintJob> CreatePrintJobAsync(PrintJob printJob, int userId);
        Task UpdatePrintJobStatusAsync(int printJobId, string status, int userId);
        Task DeletePrintJobAsync(int printJobId, int userId);
    }
} 