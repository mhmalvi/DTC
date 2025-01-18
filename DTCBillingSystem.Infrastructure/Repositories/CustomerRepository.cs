using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByMeterNumberAsync(string meterNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.MeterNumber == meterNumber);
        }

        public async Task<Customer?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name.Contains(name));
        }

        public async Task<bool> IsMeterNumberUniqueAsync(string meterNumber)
        {
            return !await _dbSet.AnyAsync(c => c.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize, string? searchTerm = null, CustomerType? type = null, bool? isActive = null, string? zone = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => c.Name.Contains(searchTerm) || c.MeterNumber.Contains(searchTerm));

            if (type.HasValue)
                query = query.Where(c => c.CustomerType == type.Value);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(zone))
                query = query.Where(c => c.Zone == zone);

            return await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null, CustomerType? type = null, bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => c.Name.Contains(searchTerm) || c.MeterNumber.Contains(searchTerm));

            if (type.HasValue)
                query = query.Where(c => c.CustomerType == type.Value);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            return await query.CountAsync();
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeCustomerId.HasValue)
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            
            return !await query.AnyAsync(c => c.AccountNumber == accountNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zone)
        {
            return await _dbSet
                .Where(c => c.Zone == zone)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _dbSet.FindAsync(id);
            if (customer == null)
                return false;

            _dbSet.Remove(customer);
            return true;
        }

        public async Task<bool> IsShopNoUniqueAsync(string shopNo, int? excludeCustomerId = null)
        {
            var query = _dbSet.AsQueryable();
            if (excludeCustomerId.HasValue)
                query = query.Where(c => c.Id != excludeCustomerId.Value);
            
            return !await query.AnyAsync(c => c.ShopNo == shopNo);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.Name.Contains(searchTerm) || 
                           c.MeterNumber.Contains(searchTerm) || 
                           c.ShopNo.Contains(searchTerm) ||
                           c.Zone.Contains(searchTerm))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
} 