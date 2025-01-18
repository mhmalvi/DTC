using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<IEnumerable<Customer>> GetCustomersByTypeAsync(CustomerType type);
        Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode);
        Task<Customer?> GetCustomerByAccountNumberAsync(string accountNumber);
        Task<Customer?> GetCustomerByMeterNumberAsync(string meterNumber);
        Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize, string? searchText = null, CustomerType? customerType = null, bool? isActive = null, string? sortBy = null);
        Task<int> GetTotalCustomersCountAsync(string? searchText = null, CustomerType? customerType = null, bool? isActive = null);
        Task<bool> DeactivateCustomerAsync(int id);
        Task<bool> ActivateCustomerAsync(int id);
    }
} 