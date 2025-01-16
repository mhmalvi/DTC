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
        private const int SYSTEM_USER_ID = 1;

        public CustomerService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(
            int pageNumber,
            int pageSize,
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null,
            string? sortBy = null)
        {
            return await _unitOfWork.Customers.GetCustomersAsync(
                pageNumber,
                pageSize,
                searchText,
                customerType,
                isActive,
                sortBy);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            // Get all customers by using GetCustomersAsync with max values
            return await GetCustomersAsync(1, int.MaxValue);
        }

        public async Task<int> GetTotalCustomersCountAsync(
            string? searchText = null,
            CustomerType? customerType = null,
            bool? isActive = null)
        {
            return await _unitOfWork.Customers.GetTotalCountAsync(searchText, customerType, isActive);
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
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
                int.TryParse(customer.CreatedBy, out int userId) ? userId : SYSTEM_USER_ID,
                AuditAction.Create.ToString(),
                "Customer created");

            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            customer.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            int userId = int.TryParse(customer.LastModifiedBy, out int id) ? id : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Customer",
                customer.Id.ToString(),
                userId,
                AuditAction.Update.ToString());

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
                SYSTEM_USER_ID,
                AuditAction.Delete.ToString());

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

            int userId = int.TryParse(customer.LastModifiedBy, out int parsedId) ? parsedId : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Customer",
                id.ToString(),
                userId,
                AuditAction.Update.ToString(),
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

            int userId = int.TryParse(customer.LastModifiedBy, out int parsedId) ? parsedId : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Customer",
                id.ToString(),
                userId,
                AuditAction.Update.ToString(),
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

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _unitOfWork.Customers.FindAsync(c => c.IsActive);
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (!await IsAccountNumberUniqueAsync(customer.AccountNumber))
                throw new InvalidOperationException("Account number must be unique");

            customer.CreatedAt = DateTime.UtcNow;
            customer.IsActive = true;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Customer",
                customer.Id.ToString(),
                int.TryParse(customer.CreatedBy, out int userId) ? userId : SYSTEM_USER_ID,
                AuditAction.Create.ToString(),
                "Customer created");

            return customer;
        }
    }
} 