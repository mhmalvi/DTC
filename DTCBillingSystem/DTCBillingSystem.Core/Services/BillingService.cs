using System;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public BillingService(
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<MonthlyBill> GenerateBillAsync(int customerId, int userId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException("Customer not found", nameof(customerId));
            }

            var latestReading = await _unitOfWork.MeterReadings.GetLatestReadingAsync(customerId);
            if (latestReading == null)
            {
                throw new InvalidOperationException("No meter reading found for customer");
            }

            var previousBills = await _unitOfWork.MonthlyBills.FindAsync(b => b.CustomerId == customerId);
            var previousBill = previousBills.OrderByDescending(b => b.BillDate).FirstOrDefault();

            var bill = new MonthlyBill
            {
                CustomerId = customerId,
                BillNumber = await GenerateBillNumberAsync(),
                BillDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                BillingPeriod = $"{DateTime.UtcNow:MMMM yyyy}",
                PreviousReading = previousBill?.CurrentReading ?? 0,
                CurrentReading = latestReading.Reading,
                Consumption = latestReading.Reading - (previousBill?.CurrentReading ?? 0),
                RatePerUnit = 10.0m, // TODO: Get from configuration
                Status = BillStatus.Draft,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            bill.BasicCharge = bill.Consumption * bill.RatePerUnit;
            bill.TaxAmount = bill.BasicCharge * 0.1m; // 10% tax
            bill.TotalAmount = bill.BasicCharge + bill.TaxAmount;

            await _unitOfWork.MonthlyBills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync("MonthlyBill", bill.Id.ToString(), userId.ToString(), AuditAction.Create);

            return bill;
        }

        public async Task<PaymentRecord> ProcessPaymentAsync(int billId, decimal amount, PaymentMethod paymentMethod, string transactionId, int userId)
        {
            var bill = await _unitOfWork.MonthlyBills.GetByIdAsync(billId);
            if (bill == null)
            {
                throw new ArgumentException("Bill not found", nameof(billId));
            }

            var payment = new PaymentRecord
            {
                MonthlyBillId = billId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                Status = PaymentStatus.Completed,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PaymentRecords.AddAsync(payment);

            var totalPaid = await GetTotalPaidForBillAsync(billId);
            bill.Status = totalPaid + amount >= bill.TotalAmount ? BillStatus.Paid : BillStatus.PartiallyPaid;

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogAsync("PaymentRecord", payment.Id.ToString(), userId.ToString(), AuditAction.Create);

            return payment;
        }

        private async Task<decimal> GetTotalPaidForBillAsync(int billId)
        {
            var payments = await _unitOfWork.PaymentRecords.FindAsync(p => p.MonthlyBillId == billId && p.Status == PaymentStatus.Completed);
            return payments.Sum(p => p.Amount);
        }

        private async Task<string> GenerateBillNumberAsync()
        {
            var lastBill = (await _unitOfWork.MonthlyBills.FindAsync(_ => true))
                .OrderByDescending(b => b.BillNumber)
                .FirstOrDefault();

            if (lastBill == null || string.IsNullOrEmpty(lastBill.BillNumber))
            {
                return "BILL-00001";
            }

            var currentNumber = int.Parse(lastBill.BillNumber.Split('-')[1]);
            return $"BILL-{(currentNumber + 1):D5}";
        }
    }
} 