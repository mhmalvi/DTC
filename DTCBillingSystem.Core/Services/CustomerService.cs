using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = unitOfWork.Customers;
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            // Validate unique constraints
            if (await IsAccountNumberTakenAsync(customer.AccountNumber))
                throw new InvalidOperationException("Account number is already taken.");

            if (await IsMeterNumberTakenAsync(customer.MeterNumber))
                throw new InvalidOperationException("Meter number is already taken.");

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id)
                ?? throw new InvalidOperationException("Customer not found.");

            // Check if account number is taken by another customer
            if (customer.AccountNumber != existingCustomer.AccountNumber &&
                await IsAccountNumberTakenAsync(customer.AccountNumber))
                throw new InvalidOperationException("Account number is already taken.");

            // Check if meter number is taken by another customer
            if (customer.MeterNumber != existingCustomer.MeterNumber &&
                await IsMeterNumberTakenAsync(customer.MeterNumber))
                throw new InvalidOperationException("Meter number is already taken.");

            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            if (await HasBillsAsync(id))
                throw new InvalidOperationException("Cannot delete customer with existing bills.");

            await _customerRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private async Task<bool> IsAccountNumberTakenAsync(string accountNumber)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Any(c => c.AccountNumber == accountNumber);
        }

        private async Task<bool> IsMeterNumberTakenAsync(string meterNumber)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Any(c => c.MeterNumber == meterNumber);
        }

        private async Task<bool> HasBillsAsync(int customerId)
        {
            var bills = await _unitOfWork.MonthlyBills.GetAllAsync();
            return bills.Any(b => b.CustomerId == customerId);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Where(c => c.IsActive);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByTypeAsync(CustomerType type)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Where(c => c.CustomerType == type);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Where(c => c.ZoneCode == zoneCode);
        }

        public async Task<Customer?> GetCustomerByAccountNumberAsync(string accountNumber)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.FirstOrDefault(c => c.AccountNumber == accountNumber);
        }

        public async Task<Customer?> GetCustomerByMeterNumberAsync(string meterNumber)
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.FirstOrDefault(c => c.MeterNumber == meterNumber);
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize, string? searchText = null, CustomerType? customerType = null, bool? isActive = null, string? sortBy = null)
        {
            return await _customerRepository.GetCustomersAsync(pageNumber, pageSize, searchText, customerType, isActive, sortBy);
        }

        public async Task<int> GetTotalCustomersCountAsync(string? searchText = null, CustomerType? customerType = null, bool? isActive = null)
        {
            return await _customerRepository.GetTotalCountAsync(searchText, customerType, isActive);
        }

        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null || !customer.IsActive)
                return false;

            customer.IsActive = false;
            customer.LastModifiedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null || customer.IsActive)
                return false;

            customer.IsActive = true;
            customer.LastModifiedAt = DateTime.UtcNow;
            await _customerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
} 