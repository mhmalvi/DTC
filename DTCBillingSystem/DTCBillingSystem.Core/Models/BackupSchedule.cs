using System;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BackupSchedule : BaseEntity
    {
        public string Name { get; set; }
        public BackupType Type { get; set; }
        public string CronExpression { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string BackupPath { get; set; }
        public int RetentionDays { get; set; }
        public bool CompressBackup { get; set; }
    }
} 