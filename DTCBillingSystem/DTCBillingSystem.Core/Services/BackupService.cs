using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public BackupService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<BackupInfo> CreateBackupAsync(int scheduleId, int userId)
        {
            var schedule = await _unitOfWork.BackupSchedules.GetByIdAsync(scheduleId)
                ?? throw new ArgumentException($"Backup schedule with ID {scheduleId} not found.");

            var backup = new BackupInfo
            {
                ScheduleId = scheduleId,
                Schedule = schedule,
                StartTime = DateTime.UtcNow,
                Status = "Pending",
                BackupPath = Path.Combine(schedule.BackupPath, $"backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                CreatedBy = userId.ToString()
            };

            await _unitOfWork.BackupInfos.AddAsync(backup);
            await _unitOfWork.SaveChangesAsync();

            return backup;
        }

        public async Task UpdateBackupStatusAsync(int backupId, string status, int userId)
        {
            var backup = await _unitOfWork.BackupInfos.GetByIdAsync(backupId)
                ?? throw new ArgumentException($"Backup with ID {backupId} not found.");

            backup.Status = status;
            backup.LastModifiedAt = DateTime.UtcNow;
            backup.LastModifiedBy = userId.ToString();

            if (status == "Completed" || status == "Failed")
            {
                backup.EndTime = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
} 