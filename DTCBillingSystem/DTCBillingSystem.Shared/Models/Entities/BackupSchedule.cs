using System;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BackupSchedule : BaseEntity
    {
        public string Name { get; set; }
        public BackupFrequency Frequency { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime LastBackup { get; set; }
        public DateTime NextBackup { get; set; }
        public string BackupPath { get; set; }
        public int RetentionDays { get; set; }
    }
} 