using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IPrintService _printService;
        private const int SYSTEM_USER_ID = 1;

        public BillingService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IPrintService printService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _printService = printService ?? throw new ArgumentNullException(nameof(printService));
        }

        public async Task<MonthlyBill> GenerateBillAsync(MonthlyBill bill)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            if (!customer.IsActive)
                throw new InvalidOperationException("Cannot generate bill for inactive customer");

            bill.CreatedAt = DateTime.UtcNow;
            bill.Status = BillStatus.Pending;

            await _unitOfWork.MonthlyBills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            int userId = int.TryParse(bill.CreatedBy, out int id) ? id : SYSTEM_USER_ID;

            await _auditService.LogAsync(
                "Bill",
                bill.Id.ToString(),
                userId,
                AuditAction.Create.ToString(),
                $"Bill generated for Customer {bill.CustomerId}");

            return bill;
        }

        public async Task<MonthlyBill?> GetBillByIdAsync(int id)
        {
            return await _unitOfWork.MonthlyBills.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByCustomerAsync(int customerId)
        {
            return await _unitOfWork.MonthlyBills.GetBillsByCustomerIdAsync(customerId);
        }

        public async Task<MonthlyBill> UpdateBillAsync(MonthlyBill bill)
        {
            if (bill == null)
                throw new ArgumentNullException(nameof(bill));

            var existingBill = await _unitOfWork.MonthlyBills.GetByIdAsync(bill.Id);
            if (existingBill == null)
                throw new InvalidOperationException("Bill not found");

            if (existingBill.Status == BillStatus.Paid)
                throw new InvalidOperationException("Cannot update paid bill");

            bill.LastModifiedAt = DateTime.UtcNow;
            await _unitOfWork.MonthlyBills.UpdateAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Bill",
                bill.Id.ToString(),
                int.TryParse(bill.LastModifiedBy, out int userId) ? userId : SYSTEM_USER_ID,
                AuditAction.Update.ToString(),
                "Bill updated");

            return bill;
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(id);
            if (bill == null)
                return false;

            if (bill.Status == BillStatus.Paid)
                throw new InvalidOperationException("Cannot delete paid bill");

            await _unitOfWork.MonthlyBills.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Bill",
                id.ToString(),
                SYSTEM_USER_ID,
                AuditAction.Delete.ToString(),
                "Bill deleted");

            return true;
        }

        public async Task<bool> MarkBillAsPaidAsync(int id, string paymentReference)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(id);
            if (bill == null)
                return false;

            if (bill.Status == BillStatus.Paid)
                throw new InvalidOperationException("Bill is already marked as paid");

            bill.Status = BillStatus.Paid;
            bill.PaymentReference = paymentReference;
            bill.PaidDate = DateTime.UtcNow;
            bill.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                "Bill",
                id.ToString(),
                SYSTEM_USER_ID,
                AuditAction.Update.ToString(),
                $"Bill marked as paid. Payment Reference: {paymentReference}");

            return true;
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId)
        {
            return await _unitOfWork.MonthlyBills.GetBillsByCustomerIdAsync(customerId);
        }

        public async Task<MonthlyBill?> GetBillDetailsAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId);
            if (bill != null)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId);
                bill.Customer = customer ?? throw new InvalidOperationException($"Customer not found for ID: {bill.CustomerId}");
            }
            return bill;
        }

        public async Task<bool> PrintBillAsync(int billId)
        {
            var bill = await GetBillDetailsAsync(billId);
            if (bill == null)
                return false;

            await _printService.PrintBillAsync(bill);
            return true;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _unitOfWork.Customers.CountAsync();
        }

        public async Task<decimal> GetMonthlyRevenueAsync(DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bills = await _unitOfWork.MonthlyBills
                .FindAsync(b => b.Status == BillStatus.Paid &&
                               b.PaidDate >= startDate &&
                               b.PaidDate <= endDate);

            return bills.Sum(b => b.Amount);
        }

        public async Task<decimal> GetTotalOutstandingAmountAsync()
        {
            var bills = await _unitOfWork.MonthlyBills
                .FindAsync(b => b.Status == BillStatus.Pending);

            return bills.Sum(b => b.Amount);
        }
    }
} 