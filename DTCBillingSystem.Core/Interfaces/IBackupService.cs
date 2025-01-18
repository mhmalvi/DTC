using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBackupService
    {
        Task<bool> CreateBackupAsync();
        Task<IEnumerable<Backup>> GetBackupHistoryAsync();
        Task<bool> RestoreFromBackupAsync(string backupPath);
        Task<bool> DeleteBackupAsync(int backupId);
    }
} 