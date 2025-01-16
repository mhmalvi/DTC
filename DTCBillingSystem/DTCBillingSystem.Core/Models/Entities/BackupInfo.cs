using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class BackupInfo : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public BackupType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsCompressed { get; set; }
        public bool IncludesTransactionLogs { get; set; }
        public long FileSize { get; set; }
        public string DatabaseVersion { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public BackupStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
} 