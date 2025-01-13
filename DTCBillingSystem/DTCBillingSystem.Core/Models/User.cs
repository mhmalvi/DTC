using System;
using System.Collections.Generic;

namespace DTCBillingSystem.Core.Models
{
    /// <summary>
    /// Represents a user of the system
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Username for login
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Salt used in password hashing
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Whether the account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When the user last logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Number of failed login attempts
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Whether the account is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// When the account lock expires
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// User's role in the system
        /// </summary>
        public UserRole Role { get; set; }

        public User()
        {
            IsActive = true;
            FailedLoginAttempts = 0;
            IsLocked = false;
            Role = UserRole.Staff;
        }
    }

    /// <summary>
    /// Represents different user roles in the system
    /// </summary>
    public enum UserRole
    {
        Administrator,
        Manager,
        Staff,
        ReadOnly
    }
} 