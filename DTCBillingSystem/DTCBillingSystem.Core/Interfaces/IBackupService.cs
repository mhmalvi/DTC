using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<BackupResult> CreateFullBackupAsync(string backupPath);

        /// <summary>
        /// Create a differential backup of the database
        /// </summary>
        Task<BackupResult> CreateDifferentialBackupAsync(string backupPath);

        /// <summary>
        /// Restore database from backup file
        /// </summary>
        Task<RestoreResult> RestoreFromBackupAsync(string backupPath);

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
        Task<ImportResult> ImportFromJsonAsync(string jsonData);
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string BackupPath { get; set; }
        public DateTime BackupTime { get; set; }
        public long BackupSize { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class RestoreResult
    {
        public bool Success { get; set; }
        public DateTime RestoreTime { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BackupInfo
    {
        public string BackupPath { get; set; }
        public DateTime BackupTime { get; set; }
        public BackupType Type { get; set; }
        public long Size { get; set; }
        public string Description { get; set; }
    }

    public class BackupSchedule
    {
        public BackupType Type { get; set; }
        public string BackupPath { get; set; }
        public TimeSpan Time { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public int RetentionDays { get; set; }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int RecordsImported { get; set; }
        public List<string> Errors { get; set; }
    }

    public enum BackupType
    {
        Full,
        Differential,
        Transaction
    }
} 