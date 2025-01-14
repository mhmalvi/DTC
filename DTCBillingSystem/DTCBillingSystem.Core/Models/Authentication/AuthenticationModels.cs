using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Models.Authentication
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public UserRole UserRole { get; set; }
    }

    public class UserRegistrationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
    }

    public class PasswordResetResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string NewPassword { get; set; }
    }
} 