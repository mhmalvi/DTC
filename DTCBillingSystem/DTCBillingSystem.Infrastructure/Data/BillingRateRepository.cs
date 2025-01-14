using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class BillingRateRepository : BaseRepository<BillingRate>, IBillingRateRepository
    {
        public BillingRateRepository(DbContext context) : base(context)
        {
        }

        public async Task<BillingRate> GetByCustomerTypeAsync(CustomerType customerType)
        {
            return await _dbSet
                .Where(r => r.CustomerType == customerType && r.IsActive)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();
        }
    }
} 