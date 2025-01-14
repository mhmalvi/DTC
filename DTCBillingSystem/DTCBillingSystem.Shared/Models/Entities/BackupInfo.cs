using System;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class BackupInfo : BaseEntity
    {
        public int BackupScheduleId { get; set; }
        public string BackupPath { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
        public BackupSchedule Schedule { get; set; }
    }
} 