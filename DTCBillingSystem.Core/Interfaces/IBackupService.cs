using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for backup and restore operations
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Create a full backup of the database
        /// </summary>
        Task<BackupInfo> CreateFullBackupAsync(string backupPath);

        /// <summary>
        /// Create a differential backup of the database
        /// </summary>
        Task<BackupInfo> CreateDifferentialBackupAsync(string backupPath);

        /// <summary>
        /// Restore database from backup file
        /// </summary>
        Task<bool> RestoreFromBackupAsync(string backupPath);

        /// <summary>
        /// Get list of available backups
        /// </summary>
        Task<IEnumerable<BackupInfo>> GetBackupListAsync();

        /// <summary>
        /// Verify backup file integrity
        /// </summary>
        Task<bool> VerifyBackupAsync(string backupPath);

        /// <summary>
        /// Schedule automated backup
        /// </summary>
        Task ScheduleAutomatedBackupAsync(BackupSchedule schedule);

        /// <summary>
        /// Export data to JSON format
        /// </summary>
        Task<string> ExportToJsonAsync();

        /// <summary>
        /// Import data from JSON format
        /// </summary>
        Task<bool> ImportFromJsonAsync(string jsonData);

        /// <summary>
        /// Get backup history
        /// </summary>
        Task<IEnumerable<BackupInfo>> GetBackupHistoryAsync();
    }
} 