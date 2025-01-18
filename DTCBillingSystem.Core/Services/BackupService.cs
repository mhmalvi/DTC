using System;
using System.IO;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly string _backupDirectory;

        public BackupService(IUnitOfWork unitOfWork, IAuditService auditService, string backupDirectory)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _backupDirectory = backupDirectory;
        }

        public async Task<bool> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"backup_{timestamp}.bak";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                Directory.CreateDirectory(_backupDirectory);

                var backup = new Backup
                {
                    FilePath = backupPath,
                    CreatedAt = DateTime.UtcNow,
                    Status = BackupStatus.InProgress
                };

                await _unitOfWork.Backups.AddAsync(backup);
                await _unitOfWork.SaveChangesAsync();

                try
                {
                    // Perform backup operation here
                    // For now, just create an empty file
                    File.Create(backupPath).Dispose();

                    backup.Status = BackupStatus.Completed;
                    backup.LastModifiedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await _auditService.LogActivityAsync(
                        "Backup",
                        "Create",
                        backup.CreatedBy,
                        $"Created backup at {backup.FilePath}"
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    backup.Status = BackupStatus.Failed;
                    backup.ErrorMessage = ex.Message;
                    backup.LastModifiedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    await _auditService.LogActivityAsync(
                        "Backup",
                        "Create",
                        0,
                        $"Backup failed: {ex.Message}"
                    );

                    return false;
                }
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Backup",
                    "Create",
                    0,
                    $"Backup initialization failed: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<IEnumerable<Backup>> GetBackupHistoryAsync()
        {
            return await _unitOfWork.Backups.GetAllAsync();
        }

        public async Task<bool> RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }

                // Perform restore operation here
                // For now, just verify the file exists

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
                    "Restore",
                    0,
                    $"Database restore failed: {ex.Message}"
                );
                return false;
            }
        }

        public async Task<bool> DeleteBackupAsync(int backupId)
        {
            try
            {
                var backup = await _unitOfWork.Backups.GetByIdAsync(backupId);
                if (backup == null)
                {
                    return false;
                }

                try
                {
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
                        "Delete",
                        0,
                        $"Failed to delete backup: {ex.Message}"
                    );
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _auditService.LogActivityAsync(
                    "Backup",
                    "Delete",
                    0,
                    $"Failed to delete backup: {ex.Message}"
                );
                return false;
            }
        }
    }
} 