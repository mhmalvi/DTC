using System;
using System.ComponentModel.DataAnnotations;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BackupInfo : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Path { get; set; } = string.Empty;

        [Required]
        public BackupType Type { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public long SizeInBytes { get; set; }

        public bool IsVerified { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [Required]
        public string VerifiedBy { get; set; } = string.Empty;

        [Required]
        public string Checksum { get; set; } = string.Empty;

        public BackupInfo()
        {
            IsVerified = false;
            Type = BackupType.Full;
            SizeInBytes = 0;
        }
    }
} 