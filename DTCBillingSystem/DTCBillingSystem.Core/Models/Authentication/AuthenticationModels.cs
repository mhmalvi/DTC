using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Models.Authentication
{
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Username { get; set; }
        public UserRole? Role { get; set; }
        public bool RequirePasswordChange { get; set; }
        public User? User { get; set; }
    }

    public class RegistrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Username { get; set; }
    }

    public class PasswordChangeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthenticationResponse : AuthenticationResult { }
    public class RegistrationResponse : RegistrationResult { }
    public class PasswordChangeResponse : PasswordChangeResult { }
    public class PasswordResetResponse : PasswordChangeResult { }
} 