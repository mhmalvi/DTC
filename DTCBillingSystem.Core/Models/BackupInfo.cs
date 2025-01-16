using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents metadata about a database backup
    /// </summary>
    public class BackupInfo
    {
        /// <summary>
        /// Unique identifier for the backup
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the backup file
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of backup (Full or Differential)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the backup file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Status of the backup
        /// </summary>
        public BackupStatus Status { get; set; }

        /// <summary>
        /// When the backup was started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When the backup was ended
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Whether the backup is compressed
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Whether the backup includes transaction logs
        /// </summary>
        public bool IncludesTransactionLogs { get; set; }

        /// <summary>
        /// Error message associated with the backup
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Size of the backup file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Database version at the time of backup
        /// </summary>
        public string DatabaseVersion { get; set; } = string.Empty;

        /// <summary>
        /// Whether the backup has been verified for integrity
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// When the backup was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User who created the backup
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// When the backup was last modified
        /// </summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>
        /// User who last modified the backup
        /// </summary>
        public string LastModifiedBy { get; set; } = string.Empty;
    }
} 