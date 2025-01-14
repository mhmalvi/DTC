using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBackupService
    {
        Task<BackupSchedule> CreateBackupScheduleAsync(BackupSchedule schedule, int userId);
        Task<BackupInfo> CreateBackupAsync(int scheduleId, int userId);
        Task UpdateBackupStatusAsync(int backupId, string status, int userId);
        Task DeleteBackupScheduleAsync(int scheduleId, int userId);
    }
} 