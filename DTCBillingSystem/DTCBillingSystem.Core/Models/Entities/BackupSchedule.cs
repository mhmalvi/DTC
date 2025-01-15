using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class BackupSchedule : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public BackupType Type { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public BackupFrequency Frequency { get; set; }
        public DateTime? LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public bool RetainTransactionLogs { get; set; }
        public int RetentionDays { get; set; }
        public bool UseCompression { get; set; } = true;
    }
} 