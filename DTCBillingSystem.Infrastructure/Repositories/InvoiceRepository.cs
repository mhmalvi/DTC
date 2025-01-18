using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Invoice>> GetBillsByCustomerAsync(int customerId)
        {
            return await _dbSet.Where(b => b.CustomerId == customerId)
                              .OrderByDescending(b => b.CreatedAt)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueBillsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet.Where(b => b.Status == BillStatus.Pending && b.CreatedAt.Date < today.AddDays(-30))
                              .OrderBy(b => b.CreatedAt)
                              .ToListAsync();
        }
    }
} 