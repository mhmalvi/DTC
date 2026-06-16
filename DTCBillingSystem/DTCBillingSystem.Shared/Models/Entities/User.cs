using System;
using System.ComponentModel.DataAnnotations;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Models.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        
        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public User()
        {
            IsActive = true;
            Role = UserRole.Staff;
        }

        public string FullName => $"{FirstName} {LastName}";
    }
} 