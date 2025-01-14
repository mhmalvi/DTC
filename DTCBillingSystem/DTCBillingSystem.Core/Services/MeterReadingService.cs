using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly ILogger<MeterReadingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MeterReadingService(
            ILogger<MeterReadingService> logger,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<MeterReading> AddReadingAsync(int customerId, decimal reading, int userId, string notes = null)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException($"Customer with ID {customerId} not found.");
            }

            var meterReading = new MeterReading
            {
                CustomerId = customerId.ToString(),
                Reading = reading,
                ReadingDate = DateTime.UtcNow,
                ReadBy = userId.ToString(),
                Notes = notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            await _unitOfWork.MeterReadings.AddAsync(meterReading);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogCreateAsync(meterReading, userId, notes);

            return meterReading;
        }

        public async Task<IEnumerable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException($"Customer with ID {customerId} not found.");
            }

            var query = await _unitOfWork.MeterReadings.FindAsync(m => m.CustomerId == customerId.ToString());
            return query;
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException($"Customer with ID {customerId} not found.");
            }

            var reading = await _unitOfWork.MeterReadings.GetLatestReadingForCustomerAsync(customerId);
            if (reading == null)
            {
                throw new ArgumentException($"No meter readings found for customer {customerId}.");
            }

            return reading;
        }

        public async Task<MeterReading> UpdateReadingAsync(int readingId, decimal newReading, int userId, string notes = null)
        {
            var reading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId);
            if (reading == null)
            {
                throw new ArgumentException($"Meter reading with ID {readingId} not found.");
            }

            reading.Reading = newReading;
            reading.Notes = notes ?? reading.Notes;
            reading.LastModifiedAt = DateTime.UtcNow;
            reading.LastModifiedBy = userId.ToString();

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogUpdateAsync(reading, userId, notes);

            return reading;
        }

        public async Task DeleteReadingAsync(int readingId, int userId)
        {
            var reading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId);
            if (reading == null)
            {
                throw new ArgumentException($"Meter reading with ID {readingId} not found.");
            }

            await _unitOfWork.MeterReadings.DeleteAsync(reading);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogDeleteAsync(reading, userId);
        }
    }
} 