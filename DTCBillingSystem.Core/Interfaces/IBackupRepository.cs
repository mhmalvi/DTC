using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IBackupRepository : IBaseRepository<Backup>
    {
        Task<IEnumerable<Backup>> GetBackupsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Backup?> GetLatestBackupAsync();
        Task<IEnumerable<Backup>> GetFailedBackupsAsync();
        Task<bool> HasBackupForDateAsync(DateTime date);
    }
} 