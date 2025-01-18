using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly string _backupDirectory;

        public BackupService(IUnitOfWork unitOfWork, IAuditService auditService, string backupDirectory)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _backupDirectory = backupDirectory ?? throw new ArgumentNullException(nameof(backupDirectory));
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.UtcNow;
                var fileName = $"backup_{timestamp:yyyyMMdd_HHmmss}.bak";
                var filePath = Path.Combine(_backupDirectory, fileName);

                // Ensure backup directory exists
                Directory.CreateDirectory(_backupDirectory);

                var backup = new Backup
                {
                    FileName = fileName,
                    FilePath = filePath,
                    CreatedAt = timestamp,
                    LastModifiedAt = timestamp,
                    CreatedBy = 0, // System backup
                    LastModifiedBy = 0
                };

                await _unitOfWork.Backups.AddAsync(backup);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Backup",
                    "Create",
                    0,
                    $"Created backup at {backup.FilePath}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Backup",
                    "Error",
                    0,
                    $"Failed to create backup: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<IEnumerable<Backup>> GetBackupHistoryAsync()
        {
            return await _unitOfWork.Backups.GetAllAsync();
        }

        public async Task<bool> DeleteBackupAsync(int backupId)
        {
            try
            {
                var backup = await _unitOfWork.Backups.GetByIdAsync(backupId);
                if (backup == null)
                    return false;

                if (File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }

                await _unitOfWork.Backups.RemoveAsync(backup);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogActivityAsync(
                    "Backup",
                    "Delete",
                    0,
                    $"Deleted backup {backup.FilePath}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Backup",
                    "Error",
                    0,
                    $"Failed to delete backup {backupId}: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // TODO: Implement actual database restore logic here

                await _auditService.LogActivityAsync(
                    "Backup",
                    "Restore",
                    0,
                    $"Restored from backup {backupPath}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Backup",
                    "Error",
                    0,
                    $"Failed to restore from backup {backupPath}: {ex.Message}"
                );
                return false;
            }
        }
    }
} 