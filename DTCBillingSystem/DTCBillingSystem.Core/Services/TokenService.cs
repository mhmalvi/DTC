using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using DTCBillingSystem.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using UserModel = DTCBillingSystem.Core.Models.User;

namespace DTCBillingSystem.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey = "your-256-bit-secret"; // TODO: Move to configuration
        private readonly string _issuer = "DTCBillingSystem";
        private readonly string _audience = "DTCBillingSystemUsers";
        private readonly int _expirationMinutes = 60;
        private readonly HashSet<string> _revokedTokens = new();

        public string GenerateToken(UserModel user)
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
            if (_revokedTokens.Contains(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return !IsTokenExpired(token);
            }
            catch
            {
                return false;
            }
        }

        public bool IsTokenExpired(string token)
        {
            if (!token.Contains(".") || _revokedTokens.Contains(token))
                return true;

            var tokenHandler = new JwtSecurityTokenHandler();
            
            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public void RevokeToken(string token)
        {
            if (!string.IsNullOrEmpty(token) && !_revokedTokens.Contains(token))
            {
                _revokedTokens.Add(token);
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            if (_revokedTokens.Contains(token))
                return null;

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
            if (_revokedTokens.Contains(token))
                return null;

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