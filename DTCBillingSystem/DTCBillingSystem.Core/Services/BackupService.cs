using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public BackupService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<BackupSchedule> CreateBackupScheduleAsync(BackupSchedule schedule, int userId)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            schedule.CreatedAt = DateTime.UtcNow;
            schedule.CreatedBy = userId.ToString();

            await _unitOfWork.BackupSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogCreateAsync(schedule, userId);

            return schedule;
        }

        public async Task<BackupInfo> CreateBackupAsync(int scheduleId, int userId)
        {
            var schedule = await _unitOfWork.BackupSchedules.GetByIdAsync(scheduleId)
                ?? throw new ArgumentException($"Backup schedule with ID {scheduleId} not found.");

            var backup = new BackupInfo
            {
                BackupScheduleId = scheduleId,
                StartTime = DateTime.UtcNow,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            await _unitOfWork.Backups.AddAsync(backup);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogCreateAsync(backup, userId);

            return backup;
        }

        public async Task UpdateBackupStatusAsync(int backupId, string status, int userId)
        {
            var backup = await _unitOfWork.Backups.GetByIdAsync(backupId)
                ?? throw new ArgumentException($"Backup with ID {backupId} not found.");

            backup.Status = status;
            backup.LastModifiedAt = DateTime.UtcNow;
            backup.LastModifiedBy = userId.ToString();

            if (status == "Completed")
            {
                backup.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogUpdateAsync(backup, userId);
        }

        public async Task DeleteBackupScheduleAsync(int scheduleId, int userId)
        {
            var schedule = await _unitOfWork.BackupSchedules.GetByIdAsync(scheduleId)
                ?? throw new ArgumentException($"Backup schedule with ID {scheduleId} not found.");

            await _unitOfWork.BackupSchedules.DeleteAsync(schedule);
            await _unitOfWork.SaveChangesAsync();
            await _auditService.LogDeleteAsync(schedule, userId);
        }
    }
} 