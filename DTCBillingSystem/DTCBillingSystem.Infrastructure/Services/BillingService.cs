using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class BillingService : IBillingService
    {
        private readonly IMonthlyBillRepository _billRepository;
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAuditService _auditService;
        private readonly IPrintService _printService;

        public BillingService(
            IMonthlyBillRepository billRepository,
            IMeterReadingRepository meterReadingRepository,
            ICustomerRepository customerRepository,
            IAuditService auditService,
            IPrintService printService)
        {
            _billRepository = billRepository;
            _meterReadingRepository = meterReadingRepository;
            _customerRepository = customerRepository;
            _auditService = auditService;
            _printService = printService;
        }

        public async Task<MonthlyBill> GenerateBillAsync(MonthlyBill bill)
        {
            var customer = await _customerRepository.GetByIdAsync(bill.CustomerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(bill.CustomerId));

            var latestReading = await _meterReadingRepository.GetLatestReadingForCustomerAsync(bill.CustomerId);
            if (latestReading == null)
                throw new InvalidOperationException("No meter reading found for customer");

            bill.Status = BillStatus.Pending;
            bill.CreatedAt = DateTime.UtcNow;
            bill.LastModifiedAt = DateTime.UtcNow;

            await _billRepository.AddAsync(bill);
            await _auditService.LogActionAsync("Billing", bill.Id, "Generate", $"Generated bill for customer {bill.CustomerId}");

            return bill;
        }

        public async Task<MonthlyBill?> GetBillByIdAsync(int id)
        {
            return await _billRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsByCustomerAsync(int customerId)
        {
            return await _billRepository.GetBillsByCustomerIdAsync(customerId);
        }

        public async Task<MonthlyBill> UpdateBillAsync(MonthlyBill bill)
        {
            var existingBill = await _billRepository.GetByIdAsync(bill.Id);
            if (existingBill == null)
                throw new ArgumentException("Bill not found", nameof(bill.Id));

            await _billRepository.UpdateAsync(bill);
            await _auditService.LogActionAsync("Billing", bill.Id, "Update", $"Updated bill {bill.Id}");

            return bill;
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _billRepository.GetByIdAsync(id);
            if (bill == null)
                return false;

            await _billRepository.DeleteAsync(id);
            await _auditService.LogActionAsync("Billing", id, "Delete", $"Deleted bill {id}");
            return true;
        }

        public async Task<bool> MarkBillAsPaidAsync(int id, string paymentReference)
        {
            var bill = await _billRepository.GetByIdAsync(id);
            if (bill == null)
                return false;

            if (bill.Status == BillStatus.Paid)
                return false;

            bill.Status = BillStatus.Paid;
            bill.PaymentReference = paymentReference;
            bill.PaidDate = DateTime.UtcNow;
            bill.LastModifiedAt = DateTime.UtcNow;

            await _billRepository.UpdateAsync(bill);
            await _auditService.LogActionAsync("Billing", id, "Payment", $"Marked bill as paid with reference {paymentReference}");
            return true;
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId)
        {
            return await _billRepository.GetCustomerBillsAsync(customerId);
        }

        public async Task<MonthlyBill?> GetBillDetailsAsync(int billId)
        {
            return await _billRepository.GetByIdAsync(billId);
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
            return await _customerRepository.CountAsync();
        }

        public async Task<decimal> GetMonthlyRevenueAsync(DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bills = await _billRepository.GetBillsByDateRangeAsync(startDate, endDate);
            return bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.Amount);
        }

        public async Task<decimal> GetTotalOutstandingAmountAsync()
        {
            var bills = await _billRepository.GetOutstandingBillsAsync(DateTime.UtcNow);
            return bills.Sum(b => b.Amount);
        }
    }
} 