using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public BillingService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<MonthlyBill>> GenerateMonthlyBillsAsync(DateTime billingMonth)
        {
            var bills = new List<MonthlyBill>();
            var customers = await _unitOfWork.Customers.GetAllAsync();

            foreach (var customer in customers)
            {
                try
                {
                    var bill = await GenerateBillForCustomerAsync(customer.Id, billingMonth);
                    bills.Add(bill);
                }
                catch (Exception ex)
                {
                    // Log error but continue with next customer
                    // TODO: Add proper logging
                }
            }

            return bills;
        }

        public async Task<MonthlyBill> GenerateBillForCustomerAsync(int customerId, DateTime billingMonth)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            var billingRate = await _unitOfWork.BillingRates.GetByCustomerTypeAsync(customer.CustomerType)
                ?? throw new InvalidOperationException($"No billing rate found for customer type {customer.CustomerType}");

            var latestReading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(customerId);
            if (latestReading == null)
                throw new InvalidOperationException($"No meter reading found for customer {customerId}");

            var previousReading = await _unitOfWork.MeterReadings.GetPreviousReadingForCustomerAsync(customerId, latestReading.ReadingDate);
            if (previousReading == null)
                throw new InvalidOperationException($"No previous meter reading found for customer {customerId}");

            var consumption = latestReading.Reading - previousReading.Reading;
            var amount = consumption * billingRate.Rate;
            var tax = amount * 0.12m; // 12% tax
            var totalAmount = amount + tax;

            var bill = new MonthlyBill
            {
                CustomerId = customerId,
                BillNumber = GenerateBillNumber(customerId, billingMonth),
                BillingDate = billingMonth,
                DueDate = billingMonth.AddDays(30),
                PreviousReading = previousReading.Reading,
                CurrentReading = latestReading.Reading,
                Consumption = consumption,
                Amount = amount,
                TaxAmount = tax,
                TotalAmount = totalAmount,
                Status = BillStatus.Pending,
                CreatedBy = "SYSTEM"
            };

            await _unitOfWork.MonthlyBills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogCreateAsync(bill, 1); // System user ID = 1

            return bill;
        }

        public async Task<PaymentRecord> ProcessPaymentAsync(int billId, decimal amount, PaymentMethod paymentMethod, string referenceNumber)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");

            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {bill.CustomerId} not found.");

            var payment = new PaymentRecord
            {
                BillId = billId,
                PaymentAmount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = paymentMethod,
                TransactionReference = referenceNumber,
                Status = PaymentStatus.Completed,
                CreatedBy = "SYSTEM"
            };

            await _unitOfWork.PaymentRecords.AddAsync(payment);

            // Get all payments for this bill including the new one
            var payments = await _unitOfWork.PaymentRecords.FindAsync(p => p.BillId == billId);
            var totalPaid = payments.Sum(p => p.PaymentAmount) + amount;

            // Update bill status based on total paid amount
            bill.Status = totalPaid >= bill.TotalAmount ? BillStatus.Paid : BillStatus.PartiallyPaid;
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogCreateAsync(payment, 1); // System user ID = 1

            return payment;
        }

        public async Task<decimal> CalculateLatePaymentChargeAsync(int billId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId)
                ?? throw new ArgumentException($"Bill with ID {billId} not found.");

            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {bill.CustomerId} not found.");

            var billingRate = await _unitOfWork.BillingRates.GetByCustomerTypeAsync(customer.CustomerType)
                ?? throw new InvalidOperationException($"No billing rate found for customer type {customer.CustomerType}");

            if (bill.Status == BillStatus.Paid)
            {
                return 0;
            }

            var daysLate = (DateTime.UtcNow - bill.DueDate).Days;
            if (daysLate <= 0)
            {
                return 0;
            }

            var lateCharge = bill.TotalAmount * (billingRate.LatePaymentRate / 100m) * daysLate;
            // Cap late charge at 50% of bill amount
            return Math.Min(lateCharge, bill.TotalAmount * 0.5m);
        }

        private string GenerateBillNumber(int customerId, DateTime billingMonth)
        {
            return $"BILL-{customerId}-{billingMonth:yyyyMM}";
        }
    }
} 