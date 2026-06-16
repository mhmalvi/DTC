using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<MonthlyBill>> GetUnpaidBillsReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentRecord>> GetPaymentHistoryReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MeterReading>> GetMeterReadingsReportAsync(DateTime startDate, DateTime endDate);
    }
} 