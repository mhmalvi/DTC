using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBackupRepository : IRepository<BackupInfo>
    {
        Task<IEnumerable<BackupInfo>> GetRecentBackupsAsync(int count);
        Task<BackupInfo> GetLastSuccessfulBackupAsync();
        Task<IEnumerable<BackupInfo>> GetFailedBackupsAsync();
        Task UpdateBackupStatusAsync(Guid backupId, string status);
    }
} 