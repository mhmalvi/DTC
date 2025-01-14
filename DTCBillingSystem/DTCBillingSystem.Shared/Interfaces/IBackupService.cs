using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBackupService
    {
        Task<BackupInfo> CreateBackupAsync(int scheduleId, int userId);
        Task UpdateBackupStatusAsync(int backupId, string status, int userId);
    }
} 