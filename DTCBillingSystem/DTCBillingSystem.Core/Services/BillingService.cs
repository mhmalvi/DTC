using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using DTCBillingSystem.Core.Extensions;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<BillingService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IEnumerable<MonthlyBill>> GenerateMonthlyBillsAsync(DateTime billingDate)
        {
            _logger.LogInformation($"Starting monthly bill generation for {billingDate:MMMM yyyy}");
            var bills = new List<MonthlyBill>();
            
            var customers = await _unitOfWork.Customers.GetAllAsync();
            foreach (var customer in customers)
            {
                try
                {
                    var bill = await GenerateBillForCustomerAsync(customer.Id, billingDate);
                    bills.Add(bill);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate bill for customer {customer.Id}");
                }
            }

            return bills;
        }

        public async Task<MonthlyBill> GenerateBillForCustomerAsync(int customerId, DateTime billingDate)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            var latestReading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(customerId);
            if (latestReading == null)
            {
                throw new InvalidOperationException($"No meter readings found for customer {customerId}");
            }

            var billingRate = await _unitOfWork.BillingRates.GetByCustomerTypeAsync(customer.Type)
                ?? throw new InvalidOperationException($"No billing rate found for customer type {customer.Type}");

            var billNumber = await GenerateBillNumberAsync(customerId);
            var bill = new MonthlyBill
            {
                BillNumber = billNumber,
                CustomerId = customerId,
                Customer = customer,
                BillingDate = billingDate,
                DueDate = billingDate.AddDays(30),
                PreviousReading = latestReading.PreviousReading,
                CurrentReading = latestReading.CurrentReading,
                Consumption = latestReading.Consumption,
                Amount = latestReading.Consumption * billingRate.Rate,
                TaxAmount = latestReading.Consumption * billingRate.Rate * billingRate.TaxRate,
                Status = BillStatus.Pending,
                BillingPeriod = billingDate.ToString("MMMM yyyy")
            };

            bill.TotalAmount = bill.Amount + bill.TaxAmount;

            await _unitOfWork.MonthlyBills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            return bill;
        }

        public async Task<PaymentRecord> ProcessPaymentAsync(int billId, decimal amount, PaymentMethod paymentMethod, string reference)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");

            if (bill.Status == BillStatus.Paid)
            {
                throw new InvalidOperationException("Bill has already been paid");
            }

            var payment = new PaymentRecord
            {
                BillId = billId,
                Bill = bill,
                Amount = amount,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Completed,
                Reference = reference
            };

            await _unitOfWork.PaymentRecords.AddAsync(payment);

            bill.Status = amount >= bill.TotalAmount ? BillStatus.Paid : BillStatus.Pending;
            bill.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return payment;
        }

        public async Task<decimal> CalculateLatePaymentChargeAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");

            if (bill.Status == BillStatus.Paid || DateTime.UtcNow <= bill.DueDate)
            {
                return 0;
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new InvalidOperationException($"Customer with ID {bill.CustomerId} not found");

            var billingRate = await _unitOfWork.BillingRates.GetByCustomerTypeAsync(customer.Type)
                ?? throw new InvalidOperationException($"No billing rate found for customer type {customer.Type}");

            var daysLate = (DateTime.UtcNow - bill.DueDate).Days;
            return bill.TotalAmount * billingRate.LatePaymentRate * daysLate;
        }

        private async Task<string> GenerateBillNumberAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            var currentDate = DateTime.UtcNow;
            var billCount = await _unitOfWork.MonthlyBills.CountAsync(b => b.CustomerId == customerId);

            return $"{customer.CustomerCode}-{currentDate:yyyyMM}-{billCount + 1:D4}";
        }
    }
} 