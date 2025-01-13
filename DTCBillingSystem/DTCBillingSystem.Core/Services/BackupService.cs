using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;
        private readonly IAuditService _auditService;
        private readonly string _backupPath;
        private readonly string _connectionString;

        public BackupService(
            IConfiguration configuration,
            ILogger<BackupService> logger,
            IAuditService auditService)
        {
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
            
            _backupPath = _configuration["AppSettings:BackupPath"] ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                    "DTCBillingSystem", "Backups");
            _connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Ensure backup directory exists
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }

        public async Task<string> CreateBackupAsync(bool includeAttachments = true)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"DTCBillingSystem_{timestamp}.bak";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);

                // Create database backup
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var backupCommand = new SqlCommand(
                        $"BACKUP DATABASE DTCBillingSystem TO DISK = '{backupFilePath}' WITH FORMAT", 
                        connection);
                    await backupCommand.ExecuteNonQueryAsync();
                }

                // If including attachments, create a zip file with both database backup and attachments
                if (includeAttachments)
                {
                    var attachmentsPath = Path.Combine(_backupPath, "..", "Attachments");
                    if (Directory.Exists(attachmentsPath))
                    {
                        var zipFileName = $"DTCBillingSystem_Full_{timestamp}.zip";
                        var zipFilePath = Path.Combine(_backupPath, zipFileName);

                        using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            // Add database backup
                            archive.CreateEntryFromFile(backupFilePath, backupFileName);

                            // Add attachments
                            var attachmentFiles = Directory.GetFiles(attachmentsPath, "*.*", 
                                SearchOption.AllDirectories);
                            foreach (var file in attachmentFiles)
                            {
                                var relativePath = Path.GetRelativePath(attachmentsPath, file);
                                archive.CreateEntryFromFile(file, Path.Combine("Attachments", relativePath));
                            }
                        }

                        // Delete the individual backup file as it's now in the zip
                        File.Delete(backupFilePath);
                        backupFilePath = zipFilePath;
                    }
                }

                await _auditService.LogActionAsync(
                    "Backup",
                    0,
                    AuditAction.Created,
                    null,
                    $"Created {(includeAttachments ? "full" : "database")} backup: {Path.GetFileName(backupFilePath)}"
                );

                return backupFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                throw;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }

                var extension = Path.GetExtension(backupPath).ToLower();
                var isZipBackup = extension == ".zip";
                var backupFile = backupPath;

                // If it's a zip backup, extract it first
                if (isZipBackup)
                {
                    var extractPath = Path.Combine(Path.GetTempPath(), "DTCBillingSystem_Restore");
                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }
                    Directory.CreateDirectory(extractPath);

                    using (var archive = ZipFile.OpenRead(backupPath))
                    {
                        // Extract database backup
                        var dbBackup = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".bak"));
                        if (dbBackup == null)
                        {
                            throw new InvalidOperationException("No database backup found in zip file");
                        }
                        dbBackup.ExtractToFile(Path.Combine(extractPath, dbBackup.Name));
                        backupFile = Path.Combine(extractPath, dbBackup.Name);

                        // Extract attachments if they exist
                        var attachments = archive.Entries.Where(e => 
                            e.FullName.StartsWith("Attachments/", StringComparison.OrdinalIgnoreCase));
                        foreach (var attachment in attachments)
                        {
                            var destinationPath = Path.Combine(
                                _backupPath, "..", 
                                attachment.FullName);
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                            attachment.ExtractToFile(destinationPath, true);
                        }
                    }
                }

                // Restore database
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Set database to single user mode
                    var singleUserCommand = new SqlCommand(
                        "ALTER DATABASE DTCBillingSystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                        connection);
                    await singleUserCommand.ExecuteNonQueryAsync();

                    // Restore database
                    var restoreCommand = new SqlCommand(
                        $"RESTORE DATABASE DTCBillingSystem FROM DISK = '{backupFile}' WITH REPLACE",
                        connection);
                    await restoreCommand.ExecuteNonQueryAsync();

                    // Set database back to multi user mode
                    var multiUserCommand = new SqlCommand(
                        "ALTER DATABASE DTCBillingSystem SET MULTI_USER",
                        connection);
                    await multiUserCommand.ExecuteNonQueryAsync();
                }

                await _auditService.LogActionAsync(
                    "Backup",
                    0,
                    AuditAction.Restored,
                    null,
                    $"Restored {(isZipBackup ? "full" : "database")} backup: {Path.GetFileName(backupPath)}"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup from {BackupPath}", backupPath);
                throw;
            }
        }

        public async Task<List<BackupInfo>> GetBackupHistoryAsync()
        {
            try
            {
                var backups = new List<BackupInfo>();
                var files = Directory.GetFiles(_backupPath, "DTCBillingSystem_*.*")
                    .Where(f => Path.GetExtension(f).ToLower() is ".bak" or ".zip");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var isFullBackup = Path.GetExtension(file).ToLower() == ".zip";

                    backups.Add(new BackupInfo
                    {
                        FileName = fileInfo.Name,
                        CreatedAt = fileInfo.CreationTimeUtc,
                        Size = fileInfo.Length,
                        Type = isFullBackup ? BackupType.Full : BackupType.DatabaseOnly,
                        Path = file
                    });
                }

                return backups.OrderByDescending(b => b.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving backup history");
                throw;
            }
        }

        public async Task<bool> DeleteBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }

                File.Delete(backupPath);

                await _auditService.LogActionAsync(
                    "Backup",
                    0,
                    AuditAction.Deleted,
                    null,
                    $"Deleted backup: {Path.GetFileName(backupPath)}"
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup {BackupPath}", backupPath);
                throw;
            }
        }

        public async Task CleanupOldBackupsAsync(int daysToKeep)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var backups = await GetBackupHistoryAsync();
                var oldBackups = backups.Where(b => b.CreatedAt < cutoffDate);

                foreach (var backup in oldBackups)
                {
                    await DeleteBackupAsync(backup.Path);
                }

                _logger.LogInformation(
                    "Cleaned up backups older than {DaysToKeep} days",
                    daysToKeep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old backups");
                throw;
            }
        }
    }
} 