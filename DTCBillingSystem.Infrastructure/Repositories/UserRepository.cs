using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private new readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await GetFirstOrDefaultAsync(
                u => u.Username == username,
                null,
                false);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await GetFirstOrDefaultAsync(
                u => u.Email == email,
                null,
                false);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId)
        {
            var query = _context.Users.AsQueryable();
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }
            return !await query.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId)
        {
            var query = _context.Users.AsQueryable();
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }
            return !await query.AnyAsync(u => u.Email == email);
        }
    }
} 