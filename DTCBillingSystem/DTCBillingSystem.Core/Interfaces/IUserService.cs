using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Authentication;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<AuthenticationResponse> AuthenticateAsync(string username, string password);
        Task<RegistrationResponse> RegisterUserAsync(string username, string email, string password, UserRole role);
        Task<PasswordChangeResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<PasswordResetResponse> ResetPasswordAsync(string username);
    }
} 