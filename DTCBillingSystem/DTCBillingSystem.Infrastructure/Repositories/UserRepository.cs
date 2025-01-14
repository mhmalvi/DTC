using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;
using DTCBillingSystem.Shared.Interfaces;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IRepository<User>
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, string excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Username.ToLower() == username.ToLower());
            
            if (!string.IsNullOrEmpty(excludeUserId))
                query = query.Where(u => u.Id != excludeUserId);

            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, string excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());
            
            if (!string.IsNullOrEmpty(excludeUserId))
                query = query.Where(u => u.Id != excludeUserId);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string passwordHash)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !user.IsActive || user.IsLocked)
                return false;

            return user.PasswordHash == passwordHash;
        }

        public async Task IncrementFailedLoginAttemptsAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                }
                await UpdateAsync(user);
            }
        }

        public async Task ResetFailedLoginAttemptsAsync(string userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LockoutEnd = null;
                await UpdateAsync(user);
            }
        }
    }
} 