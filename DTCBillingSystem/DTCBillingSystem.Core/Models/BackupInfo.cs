using System;

namespace DTCBillingSystem.Core.Models
{
    public class BackupInfo : BaseEntity
    {
        public int ScheduleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public BackupSchedule Schedule { get; set; } = null!;
    }
} 