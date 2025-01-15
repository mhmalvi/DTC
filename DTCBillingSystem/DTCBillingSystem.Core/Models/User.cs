using System;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public string? PhoneNumber { get; set; }
        public bool RequirePasswordChange { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;

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
                PasswordHash = PasswordHash,
                PasswordSalt = PasswordSalt,
                Role = Role,
                IsActive = IsActive,
                LastLoginAt = LastLoginAt,
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