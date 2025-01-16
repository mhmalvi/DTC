using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.CompilerServices;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BackupInfoModel = DTCBillingSystem.Core.Models.BackupInfo;
using BackupInfoEntity = DTCBillingSystem.Core.Models.Entities.BackupInfo;
using BackupScheduleModel = DTCBillingSystem.Core.Models.BackupSchedule;
using BackupScheduleEntity = DTCBillingSystem.Core.Models.Entities.BackupSchedule;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly IBackupInfoRepository _backupInfoRepository;
        private readonly IBackupScheduleRepository _backupScheduleRepository;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _dbContext;

        public BackupService(
            IBackupInfoRepository backupInfoRepository,
            IBackupScheduleRepository backupScheduleRepository,
            IAuditService auditService,
            ApplicationDbContext dbContext)
        {
            _backupInfoRepository = backupInfoRepository;
            _backupScheduleRepository = backupScheduleRepository;
            _auditService = auditService;
            _dbContext = dbContext;
        }

        private BackupInfoModel ConvertToModel(BackupInfoEntity entity)
        {
            return new BackupInfoModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                FilePath = entity.FilePath,
                Status = entity.Status,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsCompressed = entity.IsCompressed,
                IncludesTransactionLogs = entity.IncludesTransactionLogs,
                ErrorMessage = entity.ErrorMessage,
                FileSize = entity.FileSize,
                DatabaseVersion = entity.DatabaseVersion,
                IsVerified = entity.IsVerified,
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy ?? string.Empty,
                LastModifiedAt = entity.UpdatedAt ?? DateTime.UtcNow,
                LastModifiedBy = entity.LastModifiedBy ?? string.Empty
            };
        }

        public async Task<BackupInfoModel> CreateFullBackupAsync(string backupPath)
        {
            var backupInfo = new BackupInfoEntity
            {
                Name = $"FullBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Full.ToString(),
                FilePath = Path.Combine(backupPath, $"full_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.InProgress,
                StartTime = DateTime.UtcNow,
                IsCompressed = true,
                IncludesTransactionLogs = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _backupInfoRepository.AddAsync(backupInfo);
                var dbName = _dbContext.Database.GetDbConnection().Database;
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "BACKUP DATABASE [@dbName] TO DISK = @p0",
                    new[] { dbName, backupInfo.FilePath });

                backupInfo.Status = BackupStatus.Completed;
                backupInfo.EndTime = DateTime.UtcNow;
                await _backupInfoRepository.UpdateAsync(backupInfo);

                await _auditService.LogActionAsync("Backup", backupInfo.Id, "Create", $"Full backup created at {backupInfo.FilePath}");
                return ConvertToModel(backupInfo);
            }
            catch (Exception ex)
            {
                backupInfo.Status = BackupStatus.Failed;
                backupInfo.EndTime = DateTime.UtcNow;
                backupInfo.ErrorMessage = ex.Message;
                await _backupInfoRepository.UpdateAsync(backupInfo);

                await _auditService.LogActionAsync("Backup", backupInfo.Id, "Error", $"Full backup failed: {ex.Message}");
                throw;
            }
        }

        public async Task<BackupInfoModel> CreateDifferentialBackupAsync(string backupPath)
        {
            var backupInfo = new BackupInfoEntity
            {
                Name = $"DiffBackup_{DateTime.UtcNow:yyyyMMddHHmmss}",
                Type = BackupType.Differential.ToString(),
                FilePath = Path.Combine(backupPath, $"diff_backup_{DateTime.UtcNow:yyyyMMddHHmmss}.bak"),
                Status = BackupStatus.InProgress,
                StartTime = DateTime.UtcNow,
                IsCompressed = true,
                IncludesTransactionLogs = false,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _backupInfoRepository.AddAsync(backupInfo);
                var dbName = _dbContext.Database.GetDbConnection().Database;
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "BACKUP DATABASE [@dbName] TO DISK = @p0 WITH DIFFERENTIAL",
                    new[] { dbName, backupInfo.FilePath });

                backupInfo.Status = BackupStatus.Completed;
                backupInfo.EndTime = DateTime.UtcNow;
                await _backupInfoRepository.UpdateAsync(backupInfo);

                await _auditService.LogActionAsync("Backup", backupInfo.Id, "Create", $"Differential backup created at {backupInfo.FilePath}");
                return ConvertToModel(backupInfo);
            }
            catch (Exception ex)
            {
                backupInfo.Status = BackupStatus.Failed;
                backupInfo.EndTime = DateTime.UtcNow;
                backupInfo.ErrorMessage = ex.Message;
                await _backupInfoRepository.UpdateAsync(backupInfo);

                await _auditService.LogActionAsync("Backup", backupInfo.Id, "Error", $"Differential backup failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                var dbName = _dbContext.Database.GetDbConnection().Database;
                var sql = $"RESTORE DATABASE [{dbName}] FROM DISK = @p0 WITH REPLACE";
                
                await _dbContext.Database.ExecuteSqlRawAsync(sql, backupPath);
                await _auditService.LogActionAsync("Backup", 0, "Restore", $"Database restored from {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Restore failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                var sql = "RESTORE VERIFYONLY FROM DISK = @p0";
                
                await _dbContext.Database.ExecuteSqlRawAsync(sql, backupPath);
                await _auditService.LogActionAsync("Backup", 0, "Verify", $"Backup verified: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Verification failed: {ex.Message}");
                return false;
            }
        }

        public async Task ScheduleAutomatedBackupAsync(BackupScheduleModel schedule)
        {
            var backupSchedule = new BackupScheduleEntity
            {
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
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            await _backupScheduleRepository.AddAsync(backupSchedule);
            await _auditService.LogActionAsync("Backup", backupSchedule.Id, "Schedule", $"Scheduled automated backup: {schedule.Name}");
        }

        public async Task<string> ExportToJsonAsync()
        {
            try
            {
                var backups = await _backupInfoRepository.GetAllAsync();
                var jsonData = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
                await _auditService.LogActionAsync("Backup", 0, "Export", "Backup data exported to JSON");
                return jsonData;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Export failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ImportFromJsonAsync(string jsonData)
        {
            try
            {
                var backups = JsonSerializer.Deserialize<List<BackupInfoEntity>>(jsonData);
                if (backups == null) return false;

                foreach (var backup in backups)
                {
                    await _backupInfoRepository.AddAsync(backup);
                }

                await _auditService.LogActionAsync("Backup", 0, "Import", "Backup data imported from JSON");
                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Failed to import from JSON: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<BackupInfoModel>> GetBackupListAsync()
        {
            var backups = await _backupInfoRepository.GetAllAsync();
            return backups.Select(ConvertToModel);
        }

        public async Task<IEnumerable<BackupInfoModel>> GetBackupHistoryAsync()
        {
            var backups = await _backupInfoRepository.GetAllAsync();
            return backups.OrderByDescending(b => b.StartTime).Select(ConvertToModel);
        }
    }
} 