using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
        string? GetUserRoleFromToken(string token);
        bool IsTokenExpired(string token);
        void RevokeToken(string token);
    }
} 