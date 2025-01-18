using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync()
        {
            return await _dbSet
                .Where(n => !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByTypeAsync(NotificationType type)
        {
            return await _dbSet
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetPendingPrintJobsAsync()
        {
            return await _dbSet.Where(n => n.Type == NotificationType.PrintJob && 
                                         n.Status == NotificationStatus.Pending)
                              .OrderBy(n => n.CreatedAt)
                              .ToListAsync();
        }
    }
} 