using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class MeterReadingService : IMeterReadingService
    {
        private readonly IMeterReadingRepository _meterReadingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAuditService _auditService;

        public MeterReadingService(
            IMeterReadingRepository meterReadingRepository,
            ICustomerRepository customerRepository,
            IAuditService auditService)
        {
            _meterReadingRepository = meterReadingRepository;
            _customerRepository = customerRepository;
            _auditService = auditService;
        }

        public async Task<MeterReading> CreateReadingAsync(MeterReading reading, int userId)
        {
            var customer = await _customerRepository.GetByIdAsync(reading.CustomerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(reading.CustomerId));

            var latestReading = await _meterReadingRepository.GetLatestReadingForCustomerAsync(reading.CustomerId);
            if (latestReading != null && reading.Reading < latestReading.Reading)
                throw new InvalidOperationException("New reading cannot be less than previous reading");

            reading.ReadBy = userId.ToString();
            reading.ReadingDate = DateTime.UtcNow;
            reading.CreatedAt = DateTime.UtcNow;
            reading.CreatedBy = userId.ToString();
            reading.Status = ReadingStatus.Pending;

            await _meterReadingRepository.AddAsync(reading);
            await _auditService.LogActionAsync("MeterReading", reading.Id, "Create", $"Added reading for customer {reading.CustomerId}");

            return reading;
        }

        public async Task<MeterReading> UpdateReadingAsync(int readingId, MeterReading reading, int userId)
        {
            var existingReading = await _meterReadingRepository.GetByIdAsync(readingId);
            if (existingReading == null)
                throw new ArgumentException("Reading not found", nameof(readingId));

            existingReading.Reading = reading.Reading;
            existingReading.Notes = reading.Notes;
            existingReading.Status = reading.Status;
            existingReading.LastModifiedAt = DateTime.UtcNow;
            existingReading.LastModifiedBy = userId.ToString();

            await _meterReadingRepository.UpdateAsync(existingReading);
            await _auditService.LogActionAsync("MeterReading", readingId, "Update", $"Updated reading {readingId}");

            return existingReading;
        }

        public async Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(customerId));

            return await _meterReadingRepository.GetReadingsForCustomerAsync(customerId);
        }

        public async Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found", nameof(customerId));

            var reading = await _meterReadingRepository.GetLatestReadingForCustomerAsync(customerId);
            if (reading == null)
                throw new InvalidOperationException($"No readings found for customer {customerId}");

            return reading;
        }

        public async Task DeleteReadingAsync(int readingId, int userId)
        {
            var reading = await _meterReadingRepository.GetByIdAsync(readingId);
            if (reading == null)
                throw new ArgumentException("Reading not found", nameof(readingId));

            await _meterReadingRepository.RemoveAsync(reading);
            await _auditService.LogActionAsync("MeterReading", readingId, "Delete", $"Deleted reading {readingId} by user {userId}");
        }
    }
} 