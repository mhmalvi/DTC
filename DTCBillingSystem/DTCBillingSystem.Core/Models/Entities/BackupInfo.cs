using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class BackupInfo : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public BackupType BackupType { get; set; }
        public string Type { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public BackupStatus Status { get; set; }
        public DateTime BackupDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCompressed { get; set; }
        public bool IncludesTransactionLogs { get; set; }
        public string? ErrorMessage { get; set; }
        public long FileSize { get; set; }
        public string DatabaseVersion { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
} 