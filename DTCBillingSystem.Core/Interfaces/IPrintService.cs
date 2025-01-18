using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPrintService
    {
        Task<bool> QueuePrintJobAsync(string documentType, string content, int userId);
        Task<bool> CompletePrintJobAsync(int notificationId, int userId);
        Task<IEnumerable<Notification>> GetPendingPrintJobsAsync();
        Task<bool> CancelPrintJobAsync(int notificationId, int userId);
    }
} 