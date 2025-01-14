using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class NotificationRepository : BaseRepository<NotificationMessage>, INotificationRepository
    {
        public NotificationRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NotificationMessage>> GetUnsentNotificationsAsync()
        {
            return await _dbSet
                .Where(n => n.Status == "Pending")
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Where(n => n.Status == status)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
} 