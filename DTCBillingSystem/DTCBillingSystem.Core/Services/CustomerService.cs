using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public CustomerService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string sortBy = null)
        {
            return await _unitOfWork.Customers.GetCustomersAsync(
                pageNumber,
                pageSize,
                searchText,
                customerType,
                isActive,
                sortBy);
        }

        public async Task<int> GetTotalCustomersCountAsync(
            string searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null)
        {
            return await _unitOfWork.Customers.GetTotalCountAsync(searchText, customerType, isActive);
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            return await _unitOfWork.Customers.GetByIdAsync(id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (!await IsAccountNumberUniqueAsync(customer.AccountNumber))
                throw new InvalidOperationException("Account number already exists");

            customer.CreatedAt = DateTime.UtcNow;
            customer.LastModifiedAt = DateTime.UtcNow;
            customer.IsActive = true;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                customer.Id.ToString(),
                customer.CreatedBy,
                AuditAction.Create);

            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
            if (existingCustomer == null)
                throw new InvalidOperationException("Customer not found");

            if (existingCustomer.AccountNumber != customer.AccountNumber &&
                !await IsAccountNumberUniqueAsync(customer.AccountNumber, customer.Id))
                throw new InvalidOperationException("Account number already exists");

            customer.LastModifiedAt = DateTime.UtcNow;
            customer.CreatedAt = existingCustomer.CreatedAt;
            customer.CreatedBy = existingCustomer.CreatedBy;

            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                customer.Id.ToString(),
                customer.LastModifiedBy,
                AuditAction.Update);

            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return false;

            // Check if customer has any bills or payments
            if (await _unitOfWork.MonthlyBills.HasBillsForCustomerAsync(id) ||
                await _unitOfWork.PaymentRecords.HasPaymentsForCustomerAsync(id))
            {
                throw new InvalidOperationException("Cannot delete customer with existing bills or payments");
            }

            await _unitOfWork.Customers.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                id.ToString(),
                "System",
                AuditAction.Delete);

            return true;
        }

        public async Task<bool> DeactivateCustomerAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return false;

            customer.IsActive = false;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                id.ToString(),
                customer.LastModifiedBy,
                AuditAction.Update,
                "Customer deactivated");

            return true;
        }

        public async Task<bool> ActivateCustomerAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return false;

            customer.IsActive = true;
            customer.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                id.ToString(),
                customer.LastModifiedBy,
                AuditAction.Update,
                "Customer activated");

            return true;
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, int? excludeCustomerId = null)
        {
            return await _unitOfWork.Customers.IsAccountNumberUniqueAsync(accountNumber, excludeCustomerId);
        }

        public async Task<IEnumerable<Customer>> GetCustomersByZoneAsync(string zoneCode)
        {
            return await _unitOfWork.Customers.GetCustomersByZoneAsync(zoneCode);
        }
    }
} 