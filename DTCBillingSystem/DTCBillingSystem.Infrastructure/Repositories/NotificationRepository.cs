using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.Infrastructure.Data;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class NotificationRepository : BaseRepository<NotificationMessage>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<NotificationSettings> GetSettingsAsync(int customerId)
        {
            return await _context.NotificationSettings
                .FirstOrDefaultAsync(s => s.CustomerId == customerId);
        }

        public async Task AddSettingsAsync(NotificationSettings settings)
        {
            await _context.NotificationSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSettingsAsync(NotificationSettings settings)
        {
            _context.NotificationSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationHistory>> GetHistoryByCustomerIdAsync(int customerId)
        {
            return await _context.NotificationHistory
                .Where(h => h.CustomerId == customerId)
                .OrderByDescending(h => h.SentAt)
                .ToListAsync();
        }

        public async Task AddHistoryAsync(NotificationHistory history)
        {
            await _context.NotificationHistory.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetPendingNotificationsAsync()
        {
            return await _context.NotificationMessages
                .Where(m => m.Status == NotificationStatus.Pending)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetScheduledNotificationsAsync(DateTime before)
        {
            return await _context.NotificationMessages
                .Where(m => m.Status == NotificationStatus.Scheduled &&
                           m.ScheduledTime <= before)
                .OrderBy(m => m.ScheduledTime)
                .ToListAsync();
        }

        public async Task AddMessageAsync(NotificationMessage message)
        {
            await _context.NotificationMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMessageAsync(NotificationMessage message)
        {
            _context.NotificationMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetMessagesByTypeAsync(
            NotificationType type,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.NotificationMessages
                .Where(m => m.Type == type);

            if (startDate.HasValue)
                query = query.Where(m => m.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.CreatedAt <= endDate.Value);

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetFailedMessagesAsync(DateTime? since = null)
        {
            var query = _context.NotificationMessages
                .Where(m => m.Status == NotificationStatus.Failed);

            if (since.HasValue)
                query = query.Where(m => m.CreatedAt >= since.Value);

            return await query
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<NotificationMessage> Messages, int TotalCount)> GetPagedMessagesAsync(
            int pageNumber,
            int pageSize,
            NotificationType? type = null,
            NotificationStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var query = _context.NotificationMessages.AsQueryable();

            if (type.HasValue)
                query = query.Where(m => m.Type == type.Value);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            if (startDate.HasValue)
                query = query.Where(m => m.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(m => m.CreatedAt <= endDate.Value);

            query = query.OrderByDescending(m => m.CreatedAt);

            return await GetPagedResponseWithTotalAsync(query, pageNumber, pageSize);
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            var message = await _context.NotificationMessages.FindAsync(messageId);
            if (message != null)
            {
                _context.NotificationMessages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationMessage>> GetUnsentMessagesAsync()
        {
            return await _context.NotificationMessages
                .Where(m => m.Status == NotificationStatus.Pending ||
                           (m.Status == NotificationStatus.Scheduled &&
                            m.ScheduledTime <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetPendingMessageCountAsync(int customerId)
        {
            return await _context.NotificationMessages
                .CountAsync(m => m.CustomerId == customerId &&
                                m.Status == NotificationStatus.Pending);
        }
    }
} 