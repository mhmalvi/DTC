using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using System;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ITokenService _tokenService;
        private User? _cachedUser;
        private string? _currentToken;

        public CurrentUserService(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_currentToken) && !_tokenService.IsTokenExpired(_currentToken);

        public User? CurrentUser => _cachedUser;

        public void SetCurrentUser(User? user)
        {
            _cachedUser = user;
            if (user != null)
            {
                _currentToken = _tokenService.GenerateToken(user);
            }
            else
            {
                _currentToken = null;
            }
        }

        public Task<User?> GetCurrentUserAsync()
        {
            return Task.FromResult(_cachedUser);
        }

        public void ClearCurrentUser()
        {
            if (_currentToken != null)
            {
                _tokenService.RevokeToken(_currentToken);
                _currentToken = null;
            }
            _cachedUser = null;
        }
    }
} 