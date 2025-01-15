using System;
using System.Threading.Tasks;
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

            await _unitOfWork.PrintJobs.RemoveAsync(printJob);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}