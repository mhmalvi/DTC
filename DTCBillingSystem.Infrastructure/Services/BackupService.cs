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
                CreatedBy = entity.CreatedBy,
                LastModifiedAt = entity.LastModifiedAt,
                LastModifiedBy = entity.LastModifiedBy
            };
        }

        public async Task<BackupInfoModel> CreateFullBackupAsync(string backupPath)
        {
            try
            {
                // Create backup directory if it doesn't exist
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Copy the SQLite database file
                var dbPath = Path.GetFullPath(_dbContext.Database.GetDbConnection().ConnectionString.Replace("Data Source=", ""));
                File.Copy(dbPath, backupPath, true);

                var backupInfo = new BackupInfoEntity
                {
                    Name = Path.GetFileName(backupPath),
                    Type = BackupType.Full.ToString(),
                    FilePath = backupPath,
                    Status = BackupStatus.Completed,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    FileSize = new FileInfo(backupPath).Length,
                    DatabaseVersion = "SQLite",
                    IsCompressed = false,
                    IncludesTransactionLogs = false,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "system"
                };

                await _backupInfoRepository.AddAsync(backupInfo);
                await _auditService.LogActionAsync("Backup", backupInfo.Id, "Create", "Created full backup");

                return ConvertToModel(backupInfo);
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Backup failed: {ex.Message}");
                throw;
            }
        }

        public async Task<BackupInfoModel> CreateDifferentialBackupAsync(string backupPath)
        {
            // For SQLite, we'll just create a full backup since SQLite doesn't support differential backups
            return await CreateFullBackupAsync(backupPath);
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }

                var dbPath = Path.GetFullPath(_dbContext.Database.GetDbConnection().ConnectionString.Replace("Data Source=", ""));
                
                // Close all connections
                await _dbContext.Database.CloseConnectionAsync();
                
                // Copy backup file to database location
                File.Copy(backupPath, dbPath, true);

                await _auditService.LogActionAsync("Backup", 0, "Restore", "Database restored from backup");
                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActionAsync("Backup", 0, "Error", $"Restore failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    return false;
                }

                // For SQLite, we'll just verify if the file exists and can be opened
                using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={backupPath}");
                await connection.OpenAsync();
                await connection.CloseAsync();

                await _auditService.LogActionAsync("Backup", 0, "Verify", "Backup verification completed");
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