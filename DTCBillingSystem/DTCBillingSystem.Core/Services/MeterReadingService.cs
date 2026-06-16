using System;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Core.Extensions;

namespace DTCBillingSystem.Core.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MeterReadingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MeterReading> CreateReadingAsync(MeterReading reading, int userId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(reading.CustomerId)
                ?? throw new ArgumentException($"Customer with ID {reading.CustomerId} not found.");

            var latestReading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(reading.CustomerId);

            reading.PreviousReading = latestReading?.CurrentReading ?? 0;
            reading.Consumption = reading.CurrentReading - reading.PreviousReading;
            reading.CreatedBy = userId.ToString();

            await _unitOfWork.MeterReadings.AddAsync(reading);
            await _unitOfWork.SaveChangesAsync();

            return reading;
        }

        public async Task<MeterReading> UpdateReadingAsync(int readingId, MeterReading reading, int userId)
        {
            var existingReading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId)
                ?? throw new ArgumentException($"Reading with ID {readingId} not found.");

            existingReading.CurrentReading = reading.CurrentReading;
            existingReading.ReadingDate = reading.ReadingDate;
            existingReading.Notes = reading.Notes;
            existingReading.LastModifiedBy = userId.ToString();
            existingReading.LastModifiedAt = DateTime.UtcNow;

            _unitOfWork.MeterReadings.Update(existingReading);
            await _unitOfWork.SaveChangesAsync();

            return existingReading;
        }

        public async Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            return await _unitOfWork.MeterReadings.GetReadingsForCustomerAsync(customerId);
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new ArgumentException($"Customer with ID {customerId} not found.");

            var reading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(customerId);
            if (reading == null)
            {
                throw new InvalidOperationException($"No readings found for customer {customerId}");
            }

            return reading;
        }

        public async Task DeleteReadingAsync(int readingId, int userId)
        {
            var reading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId)
                ?? throw new ArgumentException($"Reading with ID {readingId} not found.");

            _unitOfWork.MeterReadings.Remove(reading);
            await _unitOfWork.SaveChangesAsync();
        }
    }
} 