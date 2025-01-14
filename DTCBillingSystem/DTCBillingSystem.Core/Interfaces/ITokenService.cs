using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
        string? GetUserRoleFromToken(string token);
    }
} 