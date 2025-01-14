using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace DTCBillingSystem.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey = "your-256-bit-secret"; // TODO: Move to configuration
        private readonly string _issuer = "DTCBillingSystem";
        private readonly string _audience = "DTCBillingSystemUsers";
        private readonly int _expirationMinutes = 60;

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                return subClaim != null ? int.Parse(subClaim) : null;
            }
            catch
            {
                return null;
            }
        }

        public string? GetUserRoleFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal.FindFirst(ClaimTypes.Role)?.Value;
            }
            catch
            {
                return null;
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
} 