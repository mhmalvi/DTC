using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BackupSchedule : BaseEntity
    {
        public string Name { get; set; }
        public BackupType Type { get; set; }
        public DateTime StartDate { get; set; }
        public BackupFrequency Frequency { get; set; }
        public string RetentionPolicy { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public string Description { get; set; }
        public new string CreatedBy { get; set; }
        public new DateTime CreatedAt { get; set; }
        public new string LastModifiedBy { get; set; }
        public new DateTime LastModifiedAt { get; set; }
    }
} 