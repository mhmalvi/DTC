using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private User? _currentUser;

        public string UserId => CurrentUser?.Id.ToString() ?? string.Empty;
        public string? Username => CurrentUser?.Username;
        public bool IsAuthenticated => CurrentUser != null;
        public User? CurrentUser => _currentUser;

        public void SetCurrentUser(User? user)
        {
            _currentUser = user;
        }

        public Task<User?> GetCurrentUserAsync()
        {
            return Task.FromResult(_currentUser);
        }

        public void ClearCurrentUser()
        {
            _currentUser = null;
        }
    }
} 