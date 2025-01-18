using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrintService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PrintJob> CreatePrintJobAsync(PrintJob printJob, int userId)
        {
            printJob.CreatedBy = userId.ToString();
            printJob.CreatedAt = DateTime.UtcNow;
            printJob.Status = PrintJobStatus.Pending;

            await _unitOfWork.PrintJobs.AddAsync(printJob);
            await _unitOfWork.SaveChangesAsync();

            return printJob;
        }

        public async Task<PrintJob> UpdatePrintJobStatusAsync(int jobId, string status, int userId)
        {
            var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(jobId)
                ?? throw new InvalidOperationException($"Print job with ID {jobId} not found.");

            if (Enum.TryParse<PrintJobStatus>(status, true, out var printJobStatus))
            {
                printJob.Status = printJobStatus;
            }
            else
            {
                throw new ArgumentException($"Invalid print job status: {status}");
            }

            printJob.LastModifiedBy = userId.ToString();
            printJob.LastModifiedAt = DateTime.UtcNow;

            await _unitOfWork.PrintJobs.UpdateAsync(printJob);
            await _unitOfWork.SaveChangesAsync();

            return printJob;
        }

        public async Task DeletePrintJobAsync(int jobId, int userId)
        {
            var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(jobId)
                ?? throw new InvalidOperationException($"Print job with ID {jobId} not found.");

            await _unitOfWork.PrintJobs.DeleteAsync(printJob);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task PrintBillAsync(MonthlyBill bill)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(bill.CustomerId)
                ?? throw new InvalidOperationException($"Customer with ID {bill.CustomerId} not found.");

            var printJob = new PrintJob
            {
                DocumentType = "Bill",
                DocumentId = bill.Id.ToString(),
                Title = $"Bill - {customer.ShopNo} - {bill.BillingDate:MMM yyyy}",
                Status = PrintJobStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system" // TODO: Get current user ID
            };

            await CreatePrintJobAsync(printJob, 0); // TODO: Pass current user ID
        }

        public async Task<IEnumerable<PrintJob>> GetPrintJobsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.PrintJobs
                .GetAllAsync(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);
        }

        public async Task<PrintJob?> GetPrintJobByBillAsync(MonthlyBill bill)
        {
            return await _unitOfWork.PrintJobs
                .GetFirstOrDefaultAsync(p => p.BillId == bill.Id);
        }

        public async Task<bool> HasPrintJobForBillAsync(MonthlyBill bill)
        {
            var printJob = await _unitOfWork.PrintJobs
                .GetFirstOrDefaultAsync(p => p.BillId == bill.Id);
            return printJob != null;
        }

        public async Task<PrintJob> CreatePrintJobAsync(MonthlyBill bill, string createdBy)
        {
            var printJob = new PrintJob
            {
                BillId = bill.Id,
                Status = PrintJobStatus.Pending,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.PrintJobs.AddAsync(printJob);
            await _unitOfWork.SaveChangesAsync();

            return printJob;
        }

        public async Task<IEnumerable<PrintJob>> GetPendingPrintJobsAsync()
        {
            return await _unitOfWork.PrintJobs
                .GetAllAsync(p => p.Status == PrintJobStatus.Pending,
                    includeProperties: "Bill,Bill.Customer");
        }

        public async Task<IEnumerable<PrintJob>> GetCompletedPrintJobsAsync()
        {
            return await _unitOfWork.PrintJobs
                .GetAllAsync(p => p.Status == PrintJobStatus.Completed,
                    includeProperties: "Bill,Bill.Customer");
        }

        public async Task<IEnumerable<PrintJob>> GetFailedPrintJobsAsync()
        {
            return await _unitOfWork.PrintJobs
                .GetAllAsync(p => p.Status == PrintJobStatus.Failed,
                    includeProperties: "Bill,Bill.Customer");
        }
    }
}