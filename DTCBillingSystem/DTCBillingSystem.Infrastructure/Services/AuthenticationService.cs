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
                var user = await _userService.AuthenticateAsync(username, password);
                if (user != null)
                {
                    _currentUserService.SetCurrentUser(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Log the error details
                System.Diagnostics.Debug.WriteLine($"Authentication Error: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw new InvalidOperationException("Authentication failed. Please check your database connection.", ex);
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