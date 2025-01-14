using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByCustomerCodeAsync(string customerCode);
        Task<IEnumerable<Customer>> GetByNameAsync(string name);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersWithOverdueBillsAsync();
        Task<bool> IsCustomerCodeUniqueAsync(string customerCode);
    }
} 