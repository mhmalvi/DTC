using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.Extensions.Logging;

namespace DTCBillingSystem.Core.Services
{
    public class PrintService : IPrintService
    {
        private readonly ILogger<PrintService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public PrintService(
            ILogger<PrintService> logger,
            IUnitOfWork unitOfWork,
            IAuditService auditService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<PrintJob> CreatePrintJobAsync(PrintJob printJob, int userId)
        {
            try
            {
                printJob.CreatedAt = DateTime.UtcNow;
                printJob.Status = "Pending";
                await _unitOfWork.PrintJobs.AddAsync(printJob);
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogCreateAsync(printJob, userId);
                return printJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating print job");
                throw;
            }
        }

        public async Task UpdatePrintJobStatusAsync(int printJobId, string status, int userId)
        {
            try
            {
                var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(printJobId);
                if (printJob == null)
                {
                    throw new ArgumentException("Print job not found", nameof(printJobId));
                }

                printJob.Status = status;
                printJob.UpdatedAt = DateTime.UtcNow;
                if (status == "Completed")
                {
                    printJob.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogUpdateAsync(printJob, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating print job status for job {PrintJobId}", printJobId);
                throw;
            }
        }

        public async Task DeletePrintJobAsync(int printJobId, int userId)
        {
            try
            {
                var printJob = await _unitOfWork.PrintJobs.GetByIdAsync(printJobId);
                if (printJob == null)
                {
                    throw new ArgumentException("Print job not found", nameof(printJobId));
                }

                await _unitOfWork.PrintJobs.DeleteAsync(printJob);
                await _unitOfWork.SaveChangesAsync();
                await _auditService.LogDeleteAsync(printJob, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting print job {PrintJobId}", printJobId);
                throw;
            }
        }
    }
}