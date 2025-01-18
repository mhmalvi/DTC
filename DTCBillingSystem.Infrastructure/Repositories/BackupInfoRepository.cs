using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class BackupInfoRepository : Repository<BackupInfo>, IBackupInfoRepository
    {
        private new readonly ApplicationDbContext _context;

        public BackupInfoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BackupInfo>> GetByStatusAsync(BackupStatus status)
        {
            return await GetAllAsync(b => b.Status == status);
        }

        public async Task<IEnumerable<BackupInfo>> GetLatestBackupsAsync(int count)
        {
            return await _context.BackupInfos
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<BackupInfo?> GetLatestSuccessfulBackupAsync()
        {
            return await GetFirstOrDefaultAsync(
                b => b.Status == BackupStatus.Completed,
                null,
                false);
        }

        public async Task<IEnumerable<BackupInfo>> GetBackupsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAllAsync(
                b => b.CreatedAt >= startDate && b.CreatedAt <= endDate,
                null,
                false);
        }

        public async Task<BackupInfo?> GetLatestBackupAsync()
        {
            return await GetFirstOrDefaultAsync(
                null,
                null,
                false);
        }
    }
} 