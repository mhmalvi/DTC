using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class PrintService : IPrintService
    {
        private readonly IPrintJobRepository _printJobRepository;
        private readonly IAuditService _auditService;

        public PrintService(IPrintJobRepository printJobRepository, IAuditService auditService)
        {
            _printJobRepository = printJobRepository;
            _auditService = auditService;
        }

        public async Task<PrintJob> CreatePrintJobAsync(PrintJob printJob, int userId)
        {
            printJob.Status = PrintJobStatus.Pending;
            printJob.CreatedAt = DateTime.UtcNow;
            printJob.CreatedBy = userId.ToString();
            printJob.LastModifiedAt = DateTime.UtcNow;
            printJob.LastModifiedBy = userId.ToString();

            await _printJobRepository.AddAsync(printJob);
            await _auditService.LogActionAsync("PrintJob", printJob.Id, "Create", $"Created print job for {printJob.DocumentType}");

            return printJob;
        }

        public async Task<PrintJob> UpdatePrintJobStatusAsync(int jobId, string status, int userId)
        {
            var printJob = await _printJobRepository.GetByIdAsync(jobId);
            if (printJob == null)
                throw new ArgumentException($"Print job with ID {jobId} not found");

            if (Enum.TryParse<PrintJobStatus>(status, true, out var printJobStatus))
            {
                printJob.Status = printJobStatus;
            }
            else
            {
                throw new ArgumentException($"Invalid print job status: {status}");
            }

            printJob.LastModifiedAt = DateTime.UtcNow;
            printJob.LastModifiedBy = userId.ToString();

            await _printJobRepository.UpdateAsync(printJob);
            await _auditService.LogActionAsync("PrintJob", jobId, "StatusUpdate", $"Updated print job status to {status}");

            return printJob;
        }

        public async Task DeletePrintJobAsync(int jobId, int userId)
        {
            var printJob = await _printJobRepository.GetByIdAsync(jobId);
            if (printJob == null)
                throw new ArgumentException($"Print job with ID {jobId} not found");

            await _printJobRepository.RemoveAsync(printJob);
            await _auditService.LogActionAsync("PrintJob", jobId, "Delete", $"Deleted print job by user {userId}");
        }

        public async Task PrintBillAsync(MonthlyBill bill)
        {
            var printJob = new PrintJob
            {
                DocumentType = "Bill",
                DocumentId = bill.Id.ToString(),
                BillId = bill.Id,
                Title = $"Bill - {bill.BillingMonth:MMM yyyy}",
                Status = PrintJobStatus.Pending,
                ScheduledDate = DateTime.UtcNow
            };

            await CreatePrintJobAsync(printJob, 1); // TODO: Get actual user ID from context
            await _auditService.LogActionAsync("PrintJob", printJob.Id, "Print", $"Queued bill {bill.Id} for printing");
        }
    }
} 