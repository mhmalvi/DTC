using System;
using System.ComponentModel.DataAnnotations;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BackupSchedule : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public BackupType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public BackupFrequency Frequency { get; set; }

        [Required]
        public string RetentionPolicy { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        public DateTime? LastRun { get; set; }

        public DateTime? NextRun { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public BackupSchedule()
        {
            Enabled = true;
            StartDate = DateTime.UtcNow.AddHours(1);
            Type = BackupType.Full;
            Frequency = BackupFrequency.Daily;
            RetentionPolicy = "30 days";
            Description = "Automated backup schedule";
        }
    }
} 