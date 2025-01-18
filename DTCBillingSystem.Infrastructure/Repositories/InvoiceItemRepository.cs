using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class InvoiceItemRepository : BaseRepository<InvoiceItem>, IInvoiceItemRepository
    {
        public InvoiceItemRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _dbSet
                .Include(i => i.Invoice)
                .Where(i => i.InvoiceId == invoiceId)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByInvoiceIdAsync(int invoiceId)
        {
            var items = await GetByInvoiceIdAsync(invoiceId);
            return items.Sum(i => i.Amount);
        }
    }
} 