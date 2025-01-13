using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Infrastructure.Data.Repositories
{
    public class UserRepository : BaseRepository<User>
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

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Username.ToLower() == username.ToLower());
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());
            
            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(
            int pageIndex,
            int pageSize,
            string searchTerm = null,
            UserRole? role = null,
            bool? isActive = null)
        {
            IQueryable<User> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchTerm) ||
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.PhoneNumber.Contains(searchTerm));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Username)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string passwordHash)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !user.IsActive || user.IsLocked)
                return false;

            return user.PasswordHash == passwordHash;
        }

        public async Task IncrementFailedLoginAttemptsAsync(int userId)
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

        public async Task ResetFailedLoginAttemptsAsync(int userId)
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