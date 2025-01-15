using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IAuditService _auditService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IAuditService auditService)
        {
            _customerRepository = customerRepository;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string? sortBy = null)
        {
            return await _customerRepository.GetCustomersAsync(
                pageNumber,
                pageSize,
                searchText,
                customerType,
                isActive,
                sortBy);
        }

        public async Task<int> GetTotalCustomersCountAsync(
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null)
        {
            return await _customerRepository.GetTotalCountAsync(searchText, customerType, isActive);
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (!await _customerRepository.IsMeterNumberUniqueAsync(customer.MeterNumber))
                throw new InvalidOperationException("Meter number already exists");

            if (!await _customerRepository.IsShopNoUniqueAsync(customer.ShopNo))
                throw new InvalidOperationException("Shop number already exists");

            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _customerRepository.AddAsync(customer);
            await _auditService.LogActionAsync("Customer", customer.Id, "Create", $"Created customer {customer.Name}");

            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id);
            if (existingCustomer == null)
                throw new ArgumentException("Customer not found", nameof(customer.Id));

            if (customer.MeterNumber != existingCustomer.MeterNumber &&
                !await _customerRepository.IsMeterNumberUniqueAsync(customer.MeterNumber))
                throw new InvalidOperationException("Meter number already exists");

            if (customer.ShopNo != existingCustomer.ShopNo &&
                !await _customerRepository.IsShopNoUniqueAsync(customer.ShopNo, customer.Id))
                throw new InvalidOperationException("Shop number already exists");

            customer.LastModifiedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
            await _auditService.LogActionAsync("Customer", customer.Id, "Update", $"Updated customer {customer.Name}");

            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            await _customerRepository.DeleteAsync(id);
            await _auditService.LogActionAsync("Customer", id, "Delete", $"Deleted customer {customer.Name}");

            return true;
        }

        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            if (!customer.IsActive)
                return false;

            customer.IsActive = false;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            await _auditService.LogActionAsync("Customer", id, "Deactivate", $"Deactivated customer {customer.Name}");

            return true;
        }

        public async Task<bool> ActivateCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            if (customer.IsActive)
                return false;

            customer.IsActive = true;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _customerRepository.UpdateAsync(customer);
            await _auditService.LogActionAsync("Customer", id, "Activate", $"Activated customer {customer.Name}");

            return true;
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null)
        {
            return await _customerRepository.IsAccountNumberUniqueAsync(accountNumber, excludeCustomerId);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode)
        {
            return await _customerRepository.GetCustomersByZoneAsync(zoneCode);
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await GetCustomersAsync(1, int.MaxValue, isActive: true);
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            return await CreateCustomerAsync(customer);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await GetCustomersAsync(1, int.MaxValue);
        }
    }
} 