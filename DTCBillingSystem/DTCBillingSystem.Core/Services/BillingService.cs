using Microsoft.Extensions.Logging;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BillingService> _logger;
        private readonly IAuditService _auditService;

        public BillingService(
            IUnitOfWork unitOfWork,
            ILogger<BillingService> logger,
            IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<MonthlyBill> GenerateBillAsync(int customerId, DateTime billingMonth)
        {
            try
            {
                // Check if bill already exists for this month
                var exists = await _unitOfWork.MonthlyBillsExt.HasBillForMonthAsync(customerId, billingMonth);
                if (exists)
                {
                    throw new InvalidOperationException($"Bill already exists for customer {customerId} for {billingMonth:MMM yyyy}");
                }

                // Get customer details
                var customer = await _unitOfWork.CustomersExt.GetCustomerWithBillsAsync(customerId);
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found");
                }

                if (!customer.IsActive)
                {
                    throw new InvalidOperationException($"Cannot generate bill for inactive customer {customer.Name}");
                }

                // Get previous bill for readings
                var previousBill = await _unitOfWork.MonthlyBillsExt.GetCustomerLatestBillAsync(customerId);

                // Create new bill
                var bill = new MonthlyBill
                {
                    CustomerId = customerId,
                    BillingMonth = new DateTime(billingMonth.Year, billingMonth.Month, 1),
                    PreviousReading = previousBill?.PresentReading ?? 0,
                    ACPreviousReading = previousBill?.ACPresentReading ?? 0,
                    Status = BillStatus.Pending,
                    DueDate = DateTime.UtcNow.AddDays(30)
                };

                await _unitOfWork.MonthlyBills.AddAsync(bill);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "MonthlyBill",
                    bill.Id,
                    AuditAction.Created,
                    null,
                    $"Generated bill for {customer.Name} ({customer.ShopNo}) for {billingMonth:MMM yyyy}"
                );

                return bill;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bill for customer {CustomerId} for {BillingMonth}", 
                    customerId, billingMonth);
                throw;
            }
        }

        public async Task<MonthlyBill> UpdateBillReadingsAsync(
            int billId,
            decimal presentReading,
            decimal acPresentReading,
            decimal blowerFanCharge,
            decimal generatorCharge,
            decimal serviceCharge)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new KeyNotFoundException($"Bill with ID {billId} not found");
                }

                if (bill.Status != BillStatus.Pending)
                {
                    throw new InvalidOperationException($"Cannot update readings for bill in {bill.Status} status");
                }

                var oldValues = new
                {
                    bill.PresentReading,
                    bill.ACPresentReading,
                    bill.BlowerFanCharge,
                    bill.GeneratorCharge,
                    bill.ServiceCharge,
                    bill.TotalAmount
                };

                bill.PresentReading = presentReading;
                bill.ACPresentReading = acPresentReading;
                bill.BlowerFanCharge = blowerFanCharge;
                bill.GeneratorCharge = generatorCharge;
                bill.ServiceCharge = serviceCharge;

                // Calculate total amount
                await CalculateBillTotalAsync(bill);

                await _unitOfWork.MonthlyBills.UpdateAsync(bill);
                await _unitOfWork.SaveChangesAsync();

                var newValues = new
                {
                    bill.PresentReading,
                    bill.ACPresentReading,
                    bill.BlowerFanCharge,
                    bill.GeneratorCharge,
                    bill.ServiceCharge,
                    bill.TotalAmount
                };

                await _auditService.LogActionAsync(
                    "MonthlyBill",
                    bill.Id,
                    AuditAction.Updated,
                    oldValues,
                    newValues
                );

                return bill;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating readings for bill {BillId}", billId);
                throw;
            }
        }

        public async Task<PaymentRecord> RecordPaymentAsync(
            int billId,
            decimal amount,
            PaymentMethod paymentMethod,
            string transactionReference,
            string receivedBy,
            string notes = null)
        {
            try
            {
                var bill = await _unitOfWork.MonthlyBillsExt.GetBillWithDetailsAsync(billId);
                if (bill == null)
                {
                    throw new KeyNotFoundException($"Bill with ID {billId} not found");
                }

                if (bill.Status == BillStatus.Paid)
                {
                    throw new InvalidOperationException("Cannot add payment to a fully paid bill");
                }

                // Calculate late payment charges if applicable
                decimal lateCharges = 0;
                if (DateTime.UtcNow.Date > bill.DueDate.Date)
                {
                    lateCharges = CalculateLatePaymentCharges(bill);
                }

                var payment = new PaymentRecord
                {
                    BillId = billId,
                    AmountPaid = amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = paymentMethod,
                    TransactionReference = transactionReference,
                    LatePaymentCharges = lateCharges,
                    ReceivedBy = receivedBy,
                    Notes = notes
                };

                await _unitOfWork.PaymentRecords.AddAsync(payment);

                // Update bill status
                var totalPaid = bill.Payments.Sum(p => p.AmountPaid) + amount;
                bill.Status = totalPaid >= bill.TotalAmount ? BillStatus.Paid : BillStatus.PartiallyPaid;

                await _unitOfWork.MonthlyBills.UpdateAsync(bill);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActionAsync(
                    "PaymentRecord",
                    payment.Id,
                    AuditAction.Created,
                    null,
                    $"Payment of {amount:C2} recorded for bill {billId}"
                );

                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment for bill {BillId}", billId);
                throw;
            }
        }

        public async Task<decimal> GetCustomerOutstandingBalanceAsync(int customerId)
        {
            try
            {
                return await _unitOfWork.MonthlyBillsExt.GetTotalOutstandingAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting outstanding balance for customer {CustomerId}", customerId);
                throw;
            }
        }

        private async Task CalculateBillTotalAsync(MonthlyBill bill)
        {
            try
            {
                // Get current rates
                var electricityRate = await GetCurrentRateAsync("Electricity");
                var acRate = await GetCurrentRateAsync("AC");

                // Calculate electricity charges
                var unitsConsumed = bill.PresentReading - bill.PreviousReading;
                var electricityCharge = unitsConsumed * electricityRate;

                // Calculate AC charges
                var acUnitsConsumed = bill.ACPresentReading - bill.ACPreviousReading;
                var acCharge = acUnitsConsumed * acRate;

                // Calculate total
                bill.TotalAmount = electricityCharge + acCharge + 
                                 bill.BlowerFanCharge + bill.GeneratorCharge + 
                                 bill.ServiceCharge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating bill total for bill {BillId}", bill.Id);
                throw;
            }
        }

        private async Task<decimal> GetCurrentRateAsync(string rateType)
        {
            var currentRate = await _unitOfWork.BillingRates.FindAsync(r => 
                r.RateType == rateType && 
                r.IsActive && 
                r.EffectiveDate <= DateTime.UtcNow && 
                (!r.EndDate.HasValue || r.EndDate.Value > DateTime.UtcNow));

            var rate = currentRate.FirstOrDefault();
            if (rate == null)
            {
                throw new InvalidOperationException($"No active rate found for {rateType}");
            }

            return rate.Rate;
        }

        private decimal CalculateLatePaymentCharges(MonthlyBill bill)
        {
            var daysLate = (DateTime.UtcNow.Date - bill.DueDate.Date).Days;
            if (daysLate <= 0) return 0;

            // 2% of remaining balance per month
            var remainingBalance = bill.TotalAmount - bill.Payments.Sum(p => p.AmountPaid);
            var monthsLate = Math.Ceiling(daysLate / 30.0m);
            return remainingBalance * 0.02m * monthsLate;
        }
    }
} 