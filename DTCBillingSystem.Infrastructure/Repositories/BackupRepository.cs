using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class BackupRepository : BaseRepository<Backup>, IBackupRepository
    {
        public BackupRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Backup>> GetBackupsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Backup>> GetFailedBackupsAsync()
        {
            return await _dbSet
                .Where(b => !b.IsSuccessful)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasBackupForDateAsync(DateTime date)
        {
            return await _dbSet
                .AnyAsync(b => b.CreatedAt.Date == date.Date && b.IsSuccessful);
        }

        public async Task<Backup?> GetLatestBackupAsync()
        {
            return await _dbSet
                .Where(b => b.IsSuccessful)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
} 