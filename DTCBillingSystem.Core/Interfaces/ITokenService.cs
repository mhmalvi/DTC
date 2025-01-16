using System;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Service interface for JWT token operations
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generate a JWT token for a user
        /// </summary>
        string GenerateToken(User user);

        /// <summary>
        /// Validate a JWT token
        /// </summary>
        bool ValidateToken(string token);

        /// <summary>
        /// Check if a token has expired
        /// </summary>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Revoke a token
        /// </summary>
        void RevokeToken(string token);

        /// <summary>
        /// Extract user ID from token
        /// </summary>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// Extract user role from token
        /// </summary>
        string? GetUserRoleFromToken(string token);
    }
} 