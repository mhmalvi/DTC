using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly ILogger<BillingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public BillingService(
            ILogger<BillingService> logger,
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<MonthlyBill>> GenerateMonthlyBillsAsync(DateTime billingMonth)
        {
            try
            {
                _logger.LogInformation("Generating monthly bills for {BillingMonth}", billingMonth);
                
                var customers = await _unitOfWork.Customers.GetActiveCustomersAsync();
                var bills = new List<MonthlyBill>();

                foreach (var customer in customers)
                {
                    var bill = await GenerateBillForCustomerAsync(customer.Id, billingMonth);
                    bills.Add(bill);
                }

                return bills;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly bills");
                throw;
            }
        }

        public async Task<MonthlyBill> GenerateBillForCustomerAsync(int customerId, DateTime billingMonth)
        {
            try
            {
                _logger.LogInformation("Generating bill for customer {CustomerId} for {BillingMonth}", customerId, billingMonth);
                
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null)
                {
                    throw new ArgumentException($"Customer with ID {customerId} not found");
                }

                var readings = await _unitOfWork.MeterReadings.GetForBillingPeriodAsync(customerId, billingMonth);
                var rates = await _unitOfWork.BillingRates.GetForPeriodAsync(billingMonth);

                var bill = new MonthlyBill
                {
                    CustomerId = customerId,
                    BillingMonth = billingMonth,
                    GeneratedDate = DateTime.UtcNow,
                    DueDate = billingMonth.AddDays(30),
                    Status = BillStatus.Pending
                    // TODO: Calculate bill details based on readings and rates
                };

                await _unitOfWork.Bills.AddAsync(bill);
                await _unitOfWork.SaveChangesAsync();
                await _notificationService.SendBillGeneratedNotificationAsync(bill.Id);

                return bill;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bill for customer");
                throw;
            }
        }

        public async Task<decimal> CalculateBillAmountAsync(
            decimal presentReading,
            decimal previousReading,
            decimal acPresentReading,
            decimal acPreviousReading,
            decimal blowerFanCharge,
            DateTime billingMonth)
        {
            try
            {
                var rates = await _unitOfWork.BillingRates.GetForPeriodAsync(billingMonth);
                
                var mainConsumption = presentReading - previousReading;
                var acConsumption = acPresentReading - acPreviousReading;

                var mainCharge = mainConsumption * rates.MainRate;
                var acCharge = acConsumption * rates.ACRate;
                var totalCharge = mainCharge + acCharge + blowerFanCharge;

                return Math.Round(totalCharge, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating bill amount");
                throw;
            }
        }

        public async Task<PaymentRecord> RecordPaymentAsync(
            int billId,
            decimal amount,
            PaymentMethod paymentMethod,
            string transactionReference,
            string notes = null)
        {
            try
            {
                var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
                if (bill == null)
                {
                    throw new ArgumentException($"Bill with ID {billId} not found");
                }

                var payment = new PaymentRecord
                {
                    BillId = billId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    TransactionReference = transactionReference,
                    PaymentDate = DateTime.UtcNow,
                    Notes = notes
                };

                await _unitOfWork.Payments.AddAsync(payment);
                
                // Update bill status
                bill.PaidAmount += amount;
                if (bill.PaidAmount >= bill.TotalAmount)
                {
                    bill.Status = BillStatus.Paid;
                }
                else if (bill.PaidAmount > 0)
                {
                    bill.Status = BillStatus.PartiallyPaid;
                }

                await _unitOfWork.SaveChangesAsync();
                await _notificationService.SendPaymentReceivedNotificationAsync(payment.Id);

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment");
                throw;
            }
        }

        public async Task<IEnumerable<MonthlyBill>> GetOutstandingBillsAsync(int customerId)
        {
            try
            {
                return await _unitOfWork.Bills.GetOutstandingBillsAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outstanding bills");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentHistoryAsync(
            int customerId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.Payments.GetPaymentHistoryAsync(customerId, startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment history");
                throw;
            }
        }

        public async Task<IEnumerable<MonthlyBill>> GetBillsDueAsync(DateTime? dueDate = null)
        {
            try
            {
                return await _unitOfWork.Bills.GetBillsDueAsync(dueDate ?? DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bills due");
                throw;
            }
        }

        public async Task<decimal> CalculateLatePaymentChargesAsync(int billId)
        {
            try
            {
                var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
                if (bill == null)
                {
                    throw new ArgumentException($"Bill with ID {billId} not found");
                }

                if (bill.Status == BillStatus.Paid || DateTime.UtcNow <= bill.DueDate)
                {
                    return 0;
                }

                var daysLate = (DateTime.UtcNow - bill.DueDate).Days;
                var rates = await _unitOfWork.BillingRates.GetForPeriodAsync(bill.BillingMonth);
                
                return Math.Round(bill.TotalAmount * rates.LatePaymentRate * daysLate, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating late payment charges");
                throw;
            }
        }

        public async Task UpdateBillStatusAsync(int billId, BillStatus newStatus, string notes = null)
        {
            try
            {
                var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
                if (bill == null)
                {
                    throw new ArgumentException($"Bill with ID {billId} not found");
                }

                bill.Status = newStatus;
                if (!string.IsNullOrEmpty(notes))
                {
                    bill.Notes = notes;
                }

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogActionAsync(
                    "MonthlyBill",
                    billId,
                    AuditAction.Update,
                    0,
                    oldValues: $"Status: {bill.Status}",
                    newValues: $"Status: {newStatus}",
                    notes: notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bill status");
                throw;
            }
        }

        public async Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var bills = await _unitOfWork.Bills.GetForPeriodAsync(startDate, endDate);
                var payments = await _unitOfWork.Payments.GetForPeriodAsync(startDate, endDate);

                var summary = new BillingSummary
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalBills = bills.Count,
                    TotalBilledAmount = bills.Sum(b => b.TotalAmount),
                    TotalCollectedAmount = payments.Sum(p => p.Amount),
                    OverdueBillsCount = bills.Count(b => b.Status == BillStatus.Overdue),
                    OverdueAmount = bills.Where(b => b.Status == BillStatus.Overdue).Sum(b => b.TotalAmount - b.PaidAmount)
                };

                summary.TotalOutstandingAmount = summary.TotalBilledAmount - summary.TotalCollectedAmount;
                summary.CollectionEfficiencyPercentage = summary.TotalBilledAmount > 0
                    ? (summary.TotalCollectedAmount / summary.TotalBilledAmount) * 100
                    : 0;

                // Group payments by method
                summary.PaymentMethodSummaries = payments
                    .GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodSummary
                    {
                        Method = g.Key,
                        TransactionCount = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToList();

                // Group collections by day
                summary.DailyCollectionTotals = payments
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new DailyCollectionTotal
                    {
                        Date = g.Key,
                        TransactionCount = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating billing summary");
                throw;
            }
        }
    }
} 