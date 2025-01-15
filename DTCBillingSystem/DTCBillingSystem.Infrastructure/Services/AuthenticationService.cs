using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public AuthenticationService(
            IUserService userService,
            ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        public bool IsAuthenticated => _currentUserService.IsAuthenticated;

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var result = await _userService.AuthenticateAsync(username, password);
                if (result.Success && result.User != null)
                {
                    _currentUserService.SetCurrentUser(result.User);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            return await _currentUserService.GetCurrentUserAsync();
        }

        public Task LogoutAsync()
        {
            _currentUserService.ClearCurrentUser();
            return Task.CompletedTask;
        }
    }
} 