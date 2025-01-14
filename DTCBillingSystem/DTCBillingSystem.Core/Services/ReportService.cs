using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(ILogger<ReportService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MonthlyBill>> GetUnpaidBillsReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _unitOfWork.MonthlyBills.FindAsync(b => 
                    b.BillingDate >= startDate && 
                    b.BillingDate <= endDate && 
                    b.Status == BillStatus.Unpaid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unpaid bills report");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentRecord>> GetPaymentHistoryReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _unitOfWork.PaymentRecords.FindAsync(p => 
                    p.PaymentDate >= startDate && 
                    p.PaymentDate <= endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment history report");
                throw;
            }
        }

        public async Task<IEnumerable<MeterReading>> GetMeterReadingsReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _unitOfWork.MeterReadings.FindAsync(m => 
                    m.ReadingDate >= startDate && 
                    m.ReadingDate <= endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating meter readings report");
                throw;
            }
        }
    }
} 