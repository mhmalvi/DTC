using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? PhoneNumber { get; set; }
        public bool RequirePasswordChange { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        public User Clone()
        {
            return new User
            {
                Id = Id,
                Username = Username,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                PasswordHash = (byte[])PasswordHash.Clone(),
                PasswordSalt = (byte[])PasswordSalt.Clone(),
                Role = Role,
                IsActive = IsActive,
                LastLoginDate = LastLoginDate,
                PhoneNumber = PhoneNumber,
                RequirePasswordChange = RequirePasswordChange,
                CreatedAt = CreatedAt,
                CreatedBy = CreatedBy,
                LastModifiedAt = LastModifiedAt,
                LastModifiedBy = LastModifiedBy
            };
        }
    }
} 