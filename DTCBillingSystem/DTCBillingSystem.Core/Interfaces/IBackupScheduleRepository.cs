using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBackupScheduleRepository : IRepository<BackupSchedule>
    {
        Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync();
        Task<IEnumerable<BackupSchedule>> GetSchedulesDueByAsync(DateTime dueTime);
        Task<BackupSchedule?> GetByNameAsync(string name);
        Task<int> GetActiveSchedulesCountAsync();
    }
} 