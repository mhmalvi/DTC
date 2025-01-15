using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBackupInfoRepository : IRepository<BackupInfo>
    {
        Task<IEnumerable<BackupInfo>> GetByStatusAsync(BackupStatus status);
        Task<IEnumerable<BackupInfo>> GetLatestBackupsAsync(int count);
        Task<BackupInfo?> GetLatestSuccessfulBackupAsync();
    }
} 