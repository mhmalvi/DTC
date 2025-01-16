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
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly IAuditService _auditService;

        public CustomerService(
            ICustomerRepository customerRepository,
            IMeterReadingRepository meterReadingRepository,
            IAuditService auditService)
        {
            _customerRepository = customerRepository;
            _meterReadingRepository = meterReadingRepository;
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

        public async Task<Customer> CreateCustomerAsync(Customer customer, string meterNumber)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (string.IsNullOrWhiteSpace(customer.ShopNo))
                throw new ArgumentException("Shop number cannot be empty", nameof(customer.ShopNo));

            if (!await _customerRepository.IsShopNoUniqueAsync(customer.ShopNo))
                throw new InvalidOperationException("Shop number already exists");

            customer.IsActive = true;
            customer.CreatedAt = DateTime.UtcNow;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _customerRepository.AddAsync(customer);
            await _auditService.LogActionAsync("Customer", customer.Id, "Create", $"Created customer {customer.Name}");

            if (!string.IsNullOrEmpty(meterNumber))
            {
                var meterReading = new MeterReading
                {
                    CustomerId = customer.Id,
                    MeterNumber = meterNumber,
                    ReadingDate = DateTime.UtcNow,
                    Reading = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };

                await _meterReadingRepository.AddAsync(meterReading);
            }

            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer, string? meterNumber = null)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id);
            if (existingCustomer == null)
                throw new ArgumentException("Customer not found", nameof(customer.Id));

            if (string.IsNullOrWhiteSpace(customer.ShopNo))
                throw new ArgumentException("Shop number cannot be empty", nameof(customer.ShopNo));

            if (customer.ShopNo != existingCustomer.ShopNo &&
                !await _customerRepository.IsShopNoUniqueAsync(customer.ShopNo, customer.Id))
                throw new InvalidOperationException("Shop number already exists");

            customer.LastModifiedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
            await _auditService.LogActionAsync("Customer", customer.Id, "Update", $"Updated customer {customer.Name}");

            if (!string.IsNullOrEmpty(meterNumber))
            {
                var latestReading = await _meterReadingRepository.GetLatestReadingForCustomerAsync(customer.Id);
                if (latestReading != null && latestReading.MeterNumber != meterNumber)
                {
                    latestReading.MeterNumber = meterNumber;
                    latestReading.LastModifiedAt = DateTime.UtcNow;
                    await _meterReadingRepository.UpdateAsync(latestReading);
                }
            }

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
            return await CreateCustomerAsync(customer, string.Empty);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await GetCustomersAsync(1, int.MaxValue);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            return await CreateCustomerAsync(customer, string.Empty);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            return await UpdateCustomerAsync(customer, null);
        }
    }
} 