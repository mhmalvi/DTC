using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
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

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            customer.CreatedAt = DateTime.UtcNow;
            customer.LastModifiedAt = DateTime.UtcNow;
            customer.IsActive = true;

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "Customer",
                "Create",
                customer.CreatedBy,
                $"Created customer {customer.AccountNumber}"
            );

            return customer;
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _unitOfWork.Customers.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _unitOfWork.Customers.GetAllAsync();
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(customer.Id);
                if (existingCustomer == null)
                    return false;

                existingCustomer.FirstName = customer.FirstName;
                existingCustomer.LastName = customer.LastName;
                existingCustomer.Email = customer.Email;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Address = customer.Address;
                existingCustomer.LastModifiedAt = DateTime.UtcNow;
                existingCustomer.LastModifiedBy = customer.LastModifiedBy;

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Customer",
                    "Update",
                    customer.LastModifiedBy,
                    $"Updated customer {customer.AccountNumber}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Customer",
                    "Error",
                    customer?.LastModifiedBy ?? 0,
                    $"Failed to update customer: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id, int userId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                    return false;

                await _unitOfWork.Customers.RemoveAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Customer",
                    "Delete",
                    userId,
                    $"Deleted customer {customer.AccountNumber}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Customer",
                    "Error",
                    userId,
                    $"Failed to delete customer {id}: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> DeactivateCustomerAsync(int id, int userId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                    return false;

                if (!customer.IsActive)
                    return false;

                customer.IsActive = false;
                customer.LastModifiedAt = DateTime.UtcNow;
                customer.LastModifiedBy = userId;

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Customer",
                    "Deactivate",
                    userId,
                    $"Deactivated customer {customer.AccountNumber}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Customer",
                    "Error",
                    userId,
                    $"Failed to deactivate customer {id}: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> ActivateCustomerAsync(int id, int userId)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null)
                    return false;

                if (customer.IsActive)
                    return false;

                customer.IsActive = true;
                customer.LastModifiedAt = DateTime.UtcNow;
                customer.LastModifiedBy = userId;

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Customer",
                    "Activate",
                    userId,
                    $"Activated customer {customer.AccountNumber}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Customer",
                    "Error",
                    userId,
                    $"Failed to activate customer {id}: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            return await _unitOfWork.Customers.FindAsync(c =>
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.AccountNumber.Contains(searchTerm) ||
                c.PhoneNumber.Contains(searchTerm) ||
                c.Email.Contains(searchTerm)
            );
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _unitOfWork.Customers.FindAsync(c => c.IsActive);
        }

        public async Task<IEnumerable<Customer>> GetInactiveCustomersAsync()
        {
            return await _unitOfWork.Customers.FindAsync(c => !c.IsActive);
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize)
        {
            return await _unitOfWork.Customers.Query()
                .OrderBy(c => c.AccountNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCustomersCountAsync()
        {
            return await _unitOfWork.Customers.CountAsync();
        }
    }
} 