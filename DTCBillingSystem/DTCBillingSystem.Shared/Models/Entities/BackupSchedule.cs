using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BackupSchedule : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public BackupFrequency Frequency { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool IsEnabled { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public DateTime? NextExecutionTime { get; set; }
        public string? LastError { get; set; }
        public bool CompressBackup { get; set; }
        public bool IncludeTransactionLogs { get; set; }
    }
} 