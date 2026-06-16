using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;

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
            printJob.Status = "Pending";

            await _unitOfWork.PrintJobs.AddAsync(printJob);
            await _unitOfWork.SaveChangesAsync();

            return printJob;
        }

        public async Task<PrintJob> UpdatePrintJobStatusAsync(int jobId, string status, int userId)
        {
            var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (printJob == null)
            {
                throw new InvalidOperationException($"Print job with ID {jobId} not found.");
            }

            printJob.Status = status;
            printJob.LastModifiedBy = userId.ToString();
            printJob.LastModifiedAt = DateTime.UtcNow;

            _unitOfWork.PrintJobs.Update(printJob);
            await _unitOfWork.SaveChangesAsync();

            return printJob;
        }

        public async Task DeletePrintJobAsync(int jobId, int userId)
        {
            var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(jobId);
            if (printJob == null)
            {
                throw new InvalidOperationException($"Print job with ID {jobId} not found.");
            }

            _unitOfWork.PrintJobs.Remove(printJob);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}