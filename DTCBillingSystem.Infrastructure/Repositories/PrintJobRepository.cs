using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class PrintJobRepository : Repository<PrintJob>, IPrintJobRepository
    {
        private new readonly ApplicationDbContext _context;

        public PrintJobRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PrintJob?> GetByIdAsync(object id)
        {
            return await _context.PrintJobs.FindAsync(id);
        }

        public async Task DeleteAsync(PrintJob entity)
        {
            _context.PrintJobs.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public new async Task RemoveAsync(PrintJob printJob)
        {
            await DeleteAsync(printJob);
        }

        public async Task<IEnumerable<PrintJob>> GetPendingJobsAsync()
        {
            return await GetAllAsync(
                j => j.Status == PrintJobStatus.Pending,
                null,
                false);
        }

        public async Task<IEnumerable<PrintJob>> GetFailedJobsAsync()
        {
            return await GetAllAsync(
                j => j.Status == PrintJobStatus.Failed,
                null,
                false);
        }

        public async Task<IEnumerable<PrintJob>> GetJobsByStatusAsync(PrintJobStatus status)
        {
            return await GetAllAsync(
                j => j.Status == status,
                null,
                false);
        }
    }
} 