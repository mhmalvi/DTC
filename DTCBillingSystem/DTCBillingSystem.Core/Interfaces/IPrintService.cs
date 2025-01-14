using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPrintService
    {
        Task<PrintJob> CreatePrintJobAsync(PrintJob printJob, int userId);
        Task<PrintJob> UpdatePrintJobStatusAsync(int jobId, string status, int userId);
        Task DeletePrintJobAsync(int jobId, int userId);
    }
} 