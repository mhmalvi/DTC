using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Reports;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BillingSummary> GetBillingSummaryAsync(DateTime startDate, DateTime endDate)
        {
            // Get billing data asynchronously
            var bills = await _unitOfWork.MonthlyBills.GetBillsByDateRangeAsync(startDate, endDate);
            var payments = await _unitOfWork.PaymentRecords.GetPaymentsByDateRangeAsync(startDate, endDate);

            // Calculate outstanding amount for each bill
            var billOutstandingAmounts = bills.ToDictionary(
                b => b.Id,
                b => b.TotalAmount - payments.Where(p => p.MonthlyBillId == b.Id).Sum(p => p.Amount)
            );

            var summary = new BillingSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBillsGenerated = bills.Count(),
                TotalBillAmount = bills.Sum(b => b.TotalAmount),
                TotalPaymentsReceived = payments.Sum(p => p.Amount),
                TotalOutstandingAmount = billOutstandingAmounts.Values.Sum(),
                BillsByStatus = bills.GroupBy(b => b.Status)
                    .Select(g => new BillStatusSummary 
                    { 
                        Status = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(b => b.TotalAmount)
                    })
                    .ToList(),
                PaymentsByMethod = payments.GroupBy(p => p.PaymentMethod)
                    .Select(g => new PaymentMethodSummary
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .ToList()
            };

            return summary;
        }
    }
} 