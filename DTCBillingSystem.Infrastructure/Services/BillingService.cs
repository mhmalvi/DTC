using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class BillingService : IBillingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public BillingService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<MonthlyBill>> GetCustomerBillsAsync(int customerId)
        {
            return await _unitOfWork.MonthlyBills.GetByCustomerIdAsync(customerId);
        }

        public async Task GenerateBillsAsync(int startCustomerId, int endCustomerId)
        {
            for (int customerId = startCustomerId; customerId <= endCustomerId; customerId++)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer.Status != Core.Models.Enums.CustomerStatus.Active)
                    continue;

                var latestReading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(customerId);
                if (latestReading == null)
                    continue;

                var previousReading = await _unitOfWork.MeterReadings.GetPreviousReadingForCustomerAsync(customerId);
                if (previousReading == null)
                    continue;

                var bill = new MonthlyBill
                {
                    CustomerId = customerId,
                    BillingDate = DateTime.Now,
                    PreviousReading = previousReading.Reading,
                    CurrentReading = latestReading.Reading,
                    Consumption = latestReading.Reading - previousReading.Reading,
                    Amount = await CalculateBillAmountAsync(customerId, latestReading.Reading, previousReading.Reading),
                    IsPaid = false,
                    CreatedBy = int.Parse(_currentUserService.UserId),
                    LastModifiedBy = int.Parse(_currentUserService.UserId)
                };

                await _unitOfWork.MonthlyBills.AddAsync(bill);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<MonthlyBill> GenerateBillAsync(MonthlyBill bill)
        {
            bill.CreatedBy = int.Parse(_currentUserService.UserId);
            bill.LastModifiedBy = int.Parse(_currentUserService.UserId);
            await _unitOfWork.MonthlyBills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();
            return bill;
        }

        public async Task<decimal> CalculateBillAmountAsync(int customerId, decimal currentReading, decimal previousReading)
        {
            var consumption = currentReading - previousReading;
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

            // Apply any customer-specific discounts or rates here
            decimal rate = customer.Status == Core.Models.Enums.CustomerStatus.Active ? 10.0m : 12.0m;
            return consumption * rate;
        }
    }
} 