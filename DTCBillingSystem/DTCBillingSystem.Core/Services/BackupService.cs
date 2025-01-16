using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.Configuration;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Interfaces;
using BackupInfoModel = DTCBillingSystem.Core.Models.BackupInfo;
using BackupInfoEntity = DTCBillingSystem.Core.Models.Entities.BackupInfo;
using BackupScheduleModel = DTCBillingSystem.Core.Models.BackupSchedule;
using BackupScheduleEntity = DTCBillingSystem.Core.Models.Entities.BackupSchedule;

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

        private BackupInfoModel ConvertToBackupInfo(BackupInfoEntity entityBackup)
        {
            return new BackupInfoModel
            {
                Id = entityBackup.Id,
                Name = entityBackup.Name,
                Type = entityBackup.Type,
                FilePath = entityBackup.FilePath,
                Status = entityBackup.Status,
                StartTime = entityBackup.StartTime,
                EndTime = entityBackup.EndTime,
                IsCompressed = entityBackup.IsCompressed,
                IncludesTransactionLogs = entityBackup.IncludesTransactionLogs,
                ErrorMessage = entityBackup.ErrorMessage,
                FileSize = entityBackup.FileSize,
                DatabaseVersion = entityBackup.DatabaseVersion,
                IsVerified = entityBackup.IsVerified,
                CreatedAt = entityBackup.CreatedAt,
                CreatedBy = entityBackup.CreatedBy ?? string.Empty,
                LastModifiedAt = entityBackup.LastModifiedAt ?? DateTime.MinValue,
                LastModifiedBy = entityBackup.LastModifiedBy ?? string.Empty
            };
        }

        private BackupInfoEntity ConvertToEntityBackup(BackupInfoModel backup)
        {
            return new BackupInfoEntity
            {
                Id = backup.Id,
                Name = backup.Name,
                Type = backup.Type,
                FilePath = backup.FilePath,
                Status = backup.Status,
                StartTime = backup.StartTime,
                EndTime = backup.EndTime ?? DateTime.MinValue,
                IsCompressed = backup.IsCompressed,
                IncludesTransactionLogs = backup.IncludesTransactionLogs,
                ErrorMessage = backup.ErrorMessage,
                FileSize = backup.FileSize,
                DatabaseVersion = backup.DatabaseVersion,
                IsVerified = backup.IsVerified,
                CreatedAt = backup.CreatedAt,
                CreatedBy = backup.CreatedBy ?? string.Empty,
                LastModifiedAt = backup.LastModifiedAt ?? DateTime.MinValue,
                LastModifiedBy = backup.LastModifiedBy ?? string.Empty
            };
        }

        private BackupScheduleModel ConvertToBackupSchedule(BackupScheduleEntity entitySchedule)
        {
            return new BackupScheduleModel
            {
                Id = entitySchedule.Id,
                Name = entitySchedule.Name,
                Type = entitySchedule.Type,
                CronExpression = entitySchedule.CronExpression,
                IsEnabled = entitySchedule.IsEnabled,
                LastRunTime = entitySchedule.LastRunTime,
                NextRunTime = entitySchedule.NextRunTime,
                BackupPath = entitySchedule.BackupPath ?? string.Empty,
                RetainTransactionLogs = entitySchedule.RetainTransactionLogs,
                RetentionDays = entitySchedule.RetentionDays,
                UseCompression = entitySchedule.UseCompression,
                CreatedAt = entitySchedule.CreatedAt,
                CreatedBy = entitySchedule.CreatedBy ?? string.Empty,
                LastModifiedAt = entitySchedule.LastModifiedAt ?? DateTime.MinValue,
                LastModifiedBy = entitySchedule.LastModifiedBy ?? string.Empty
            };
        }

        private BackupScheduleEntity ConvertToEntitySchedule(BackupScheduleModel schedule)
        {
            return new BackupScheduleEntity
            {
                Id = schedule.Id,
                Name = schedule.Name,
                Type = schedule.Type,
                CronExpression = schedule.CronExpression,
                IsEnabled = schedule.IsEnabled,
                LastRunTime = schedule.LastRunTime,
                NextRunTime = schedule.NextRunTime,
                BackupPath = schedule.BackupPath,
                RetainTransactionLogs = schedule.RetainTransactionLogs,
                RetentionDays = schedule.RetentionDays,
                UseCompression = schedule.UseCompression,
                CreatedAt = schedule.CreatedAt,
                CreatedBy = schedule.CreatedBy ?? string.Empty,
                LastModifiedAt = schedule.LastModifiedAt,
                LastModifiedBy = schedule.LastModifiedBy ?? string.Empty
            };
        }

        public async Task<BackupInfoModel> CreateFullBackupAsync(string backupPath)
        {
            var backup = new BackupInfoModel
            {
                Name = $"FullBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Full,
                FilePath = Path.Combine(backupPath, $"full_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.Pending,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.MinValue,
                IsCompressed = true,
                IncludesTransactionLogs = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = string.Empty,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = string.Empty
            };

            var entityBackup = ConvertToEntityBackup(backup);
            await _unitOfWork.BackupInfos.AddAsync(entityBackup);
            await _unitOfWork.SaveChangesAsync();

            return ConvertToBackupInfo(entityBackup);
        }

        public async Task<BackupInfoModel> CreateDifferentialBackupAsync(string backupPath)
        {
            var backup = new BackupInfoModel
            {
                Name = $"DiffBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Differential,
                FilePath = Path.Combine(backupPath, $"diff_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.Pending,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.MinValue,
                IsCompressed = true,
                IncludesTransactionLogs = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = string.Empty,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = string.Empty
            };

            var entityBackup = ConvertToEntityBackup(backup);
            await _unitOfWork.BackupInfos.AddAsync(entityBackup);
            await _unitOfWork.SaveChangesAsync();

            return ConvertToBackupInfo(entityBackup);
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            // Implementation for database restore
            await Task.Run(() => {
                // Add your database restore logic here
                // For example: await _databaseProvider.RestoreAsync(backupPath);
            });
            
            return true;
        }

        public async Task<IEnumerable<BackupInfoModel>> GetBackupListAsync()
        {
            var backups = await _unitOfWork.BackupInfos.GetAllAsync();
            return backups.Select(ConvertToBackupInfo);
        }

        public async Task<bool> VerifyBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            // Implementation for backup verification
            await Task.Run(() => {
                // Add your backup verification logic here
                // For example: await _backupVerifier.VerifyAsync(backupPath);
            });
            
            return true;
        }

        public async Task ScheduleAutomatedBackupAsync(BackupScheduleModel schedule)
        {
            var entitySchedule = ConvertToEntitySchedule(schedule);
            await _unitOfWork.BackupSchedules.AddAsync(entitySchedule);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> ExportToJsonAsync()
        {
            var data = new
            {
                Customers = await _unitOfWork.Customers.GetAllAsync(),
                MonthlyBills = await _unitOfWork.MonthlyBills.GetAllAsync(),
                PaymentRecords = await _unitOfWork.PaymentRecords.GetAllAsync()
            };

            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }

        public async Task<bool> ImportFromJsonAsync(string jsonData)
        {
            try
            {
                // Implementation for importing data from JSON
                await Task.Run(() => {
                    // Add your JSON import logic here
                    // For example: await _importService.ImportDataAsync(jsonData);
                });
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<BackupInfoModel>> GetBackupHistoryAsync()
        {
            var backups = await _unitOfWork.BackupInfos.GetAllAsync();
            return backups.OrderByDescending(b => b.CreatedAt).Select(ConvertToBackupInfo);
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