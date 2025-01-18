using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class Backup : BaseEntity
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime BackupDate { get; set; }
        public BackupStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSuccessful { get; set; }
        public string? Notes { get; set; }
    }
} 