using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

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

        public async Task<BackupInfo> CreateFullBackupAsync(string backupPath)
        {
            var backup = new BackupInfo
            {
                Name = $"FullBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Full.ToString(),
                FilePath = Path.Combine(backupPath, $"full_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.Pending,
                StartTime = DateTime.UtcNow,
                IsCompressed = true,
                IncludesTransactionLogs = true
            };

            await _unitOfWork.BackupInfos.AddAsync(backup);
            await _unitOfWork.SaveChangesAsync();

            return backup;
        }

        public async Task<BackupInfo> CreateDifferentialBackupAsync(string backupPath)
        {
            var backup = new BackupInfo
            {
                Name = $"DiffBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Differential.ToString(),
                FilePath = Path.Combine(backupPath, $"diff_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.Pending,
                StartTime = DateTime.UtcNow,
                IsCompressed = true,
                IncludesTransactionLogs = false
            };

            await _unitOfWork.BackupInfos.AddAsync(backup);
            await _unitOfWork.SaveChangesAsync();

            return backup;
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            // Implementation for database restore
            // This would typically involve SQL commands or your database provider's restore functionality
            return true;
        }

        public async Task<IEnumerable<BackupInfo>> GetBackupListAsync()
        {
            return await _unitOfWork.BackupInfos.GetAllAsync();
        }

        public async Task<bool> VerifyBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            // Implementation for backup verification
            // This would typically involve checking file integrity and backup contents
            return true;
        }

        public async Task ScheduleAutomatedBackupAsync(BackupSchedule schedule)
        {
            await _unitOfWork.BackupSchedules.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> ExportToJsonAsync()
        {
            var data = new
            {
                Customers = await _unitOfWork.Customers.GetAllAsync(),
                Bills = await _unitOfWork.Bills.GetAllAsync(),
                Payments = await _unitOfWork.Payments.GetAllAsync()
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<bool> ImportFromJsonAsync(string jsonData)
        {
            try
            {
                // Implementation for importing data from JSON
                // This would typically involve deserializing and saving to database
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task UpdateBackupStatusAsync(int backupId, BackupStatus status, string userId)
        {
            var backup = await _unitOfWork.BackupInfos.GetByIdAsync(backupId)
                ?? throw new ArgumentException($"Backup with ID {backupId} not found.");

            backup.Status = status;
            backup.LastModifiedAt = DateTime.UtcNow;
            backup.LastModifiedBy = userId;

            if (status == BackupStatus.Completed || status == BackupStatus.Failed)
            {
                backup.EndTime = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
} 