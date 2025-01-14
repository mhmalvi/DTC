using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class BackupInfo : BaseEntity
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BackupType Type { get; set; }
        public string Description { get; set; }
        public new DateTime CreatedAt { get; set; }
        public new string CreatedBy { get; set; }
        public long SizeInBytes { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerifiedBy { get; set; }
        public string Checksum { get; set; }
    }
} 