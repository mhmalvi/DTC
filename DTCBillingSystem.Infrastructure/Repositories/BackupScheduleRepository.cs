using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class BackupScheduleRepository : Repository<BackupSchedule>, IBackupScheduleRepository
    {
        private new readonly ApplicationDbContext _context;

        public BackupScheduleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BackupSchedule>> GetSchedulesDueByAsync(DateTime date)
        {
            return await GetAllAsync(
                s => s.IsActive && s.NextRunTime <= date,
                null,
                false);
        }

        public async Task<BackupSchedule?> GetByNameAsync(string name)
        {
            return await GetFirstOrDefaultAsync(
                s => s.Name == name,
                null,
                false);
        }

        public async Task<int> GetActiveSchedulesCountAsync()
        {
            var schedules = await GetAllAsync(s => s.IsActive);
            return schedules.Count();
        }

        public async Task<IEnumerable<BackupSchedule>> GetActiveSchedulesAsync()
        {
            return await GetAllAsync(
                s => s.IsActive,
                null,
                false);
        }

        public async Task<BackupSchedule?> GetNextScheduleAsync()
        {
            var now = DateTime.UtcNow;
            return await GetFirstOrDefaultAsync(
                s => s.IsActive && s.NextRunTime > now,
                null,
                false);
        }
    }
} 