using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public MeterReadingService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        public async Task<MeterReading> CreateReadingAsync(MeterReading reading, int userId)
        {
            if (reading == null)
                throw new ArgumentNullException(nameof(reading));

            reading.CreatedAt = DateTime.UtcNow;
            reading.LastModifiedAt = DateTime.UtcNow;
            reading.CreatedBy = userId;
            reading.LastModifiedBy = userId;

            await _unitOfWork.MeterReadings.AddAsync(reading);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "MeterReading",
                "Create",
                userId,
                $"Recorded reading {reading.Reading} for customer {reading.CustomerId}"
            );

            return reading;
        }

        public async Task<MeterReading> UpdateReadingAsync(int readingId, MeterReading reading, int userId)
        {
            if (reading == null)
                throw new ArgumentNullException(nameof(reading));

            var existingReading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId);
            if (existingReading == null)
                throw new ArgumentException("Reading not found", nameof(readingId));

            existingReading.Reading = reading.Reading;
            existingReading.ReadingDate = reading.ReadingDate;
            existingReading.ReadBy = reading.ReadBy;
            existingReading.Source = reading.Source;
            existingReading.Status = reading.Status;
            existingReading.Notes = reading.Notes;
            existingReading.ImageUrl = reading.ImageUrl;
            existingReading.PreviousReading = reading.PreviousReading;
            existingReading.Consumption = reading.Consumption;
            existingReading.IsAnomalous = reading.IsAnomalous;
            existingReading.ValidationNotes = reading.ValidationNotes;
            existingReading.LastModifiedAt = DateTime.UtcNow;
            existingReading.LastModifiedBy = userId;

            await _unitOfWork.MeterReadings.UpdateAsync(existingReading);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "MeterReading",
                "Update",
                userId,
                $"Updated reading {readingId} for customer {existingReading.CustomerId}"
            );

            return existingReading;
        }

        public async Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(customerId));

            return (await _unitOfWork.MeterReadings.FindAsync(r => r.CustomerId == customerId)).AsQueryable();
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(customerId));

            var readings = await _unitOfWork.MeterReadings.FindAsync(r => r.CustomerId == customerId);
            return readings.OrderByDescending(r => r.ReadingDate).FirstOrDefault() 
                ?? throw new InvalidOperationException($"No readings found for customer {customerId}");
        }

        public async Task DeleteReadingAsync(int readingId, int userId)
        {
            var reading = await _unitOfWork.MeterReadings.GetByIdAsync(readingId);
            if (reading == null)
                throw new ArgumentException("Reading not found", nameof(readingId));

            await _unitOfWork.MeterReadings.RemoveAsync(reading);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "MeterReading",
                "Delete",
                userId,
                $"Deleted reading {readingId} for customer {reading.CustomerId}"
            );
        }
    }
} 