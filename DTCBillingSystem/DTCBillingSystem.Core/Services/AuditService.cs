using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace DTCBillingSystem.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly string _auditLogPath;
        private const int SYSTEM_USER_ID = 1; // Default system user ID

        public AuditService(ICurrentUserService currentUserService, IConfiguration configuration)
        {
            _currentUserService = currentUserService;
            _auditLogPath = configuration.GetValue<string>("AuditLogPath") ?? "audit.log";
        }

        public async Task LogActionAsync(string entityType, object? entityId, string action, string details)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            var auditLog = new
            {
                EntityType = entityType,
                EntityId = entityId?.ToString() ?? "N/A",
                Action = action,
                Details = details,
                UserId = currentUser?.Id ?? SYSTEM_USER_ID,
                Timestamp = DateTime.UtcNow
            };

            await LogToFileAsync(auditLog);
        }

        public async Task LogAsync(string entityType, string entityId, int userId, string action, string? details = null)
        {
            var auditLog = new
            {
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                Action = action,
                Details = details ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            await LogToFileAsync(auditLog);
        }

        private async Task LogToFileAsync(object logEntry)
        {
            var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });
            await File.AppendAllTextAsync(_auditLogPath, $"{json}{Environment.NewLine}");
        }
    }
} 