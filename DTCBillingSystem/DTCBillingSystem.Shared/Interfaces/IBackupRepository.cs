using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBackupRepository : IRepository<BackupInfo>
    {
        Task<IEnumerable<BackupInfo>> GetBackupsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<BackupSchedule> GetBackupScheduleAsync(string scheduleId);
        Task<BackupSchedule> UpdateBackupScheduleAsync(BackupSchedule schedule);
        Task<BackupSchedule> CreateBackupScheduleAsync(BackupSchedule schedule);
        Task<bool> DeleteBackupScheduleAsync(string scheduleId);
        Task<IEnumerable<BackupSchedule>> GetAllBackupSchedulesAsync();
        Task<IEnumerable<BackupSchedule>> GetActiveBackupSchedulesAsync();
        Task<IEnumerable<BackupSchedule>> GetBackupSchedulesByFrequencyAsync(BackupFrequency frequency);
        Task<BackupInfo> CreateBackupInfoAsync(BackupInfo backupInfo);
        Task<BackupInfo> UpdateBackupInfoAsync(BackupInfo backupInfo);
        Task<bool> DeleteBackupInfoAsync(string backupId);
    }
} 