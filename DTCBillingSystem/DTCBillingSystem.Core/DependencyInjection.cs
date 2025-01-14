using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using DTCBillingSystem.Core.Data;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Services;
using DTCBillingSystem.Core.Repositories;
using DTCBillingSystem.Core.Models.Enums;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services, string connectionString)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register DbContext as the base DbContext
            services.AddScoped<DbContext>(provider => provider.GetService<ApplicationDbContext>());

            // Register HttpContextAccessor
            services.AddHttpContextAccessor();

            // Register Repositories
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<Customer>, CustomerRepository>();
            services.AddScoped<IRepository<MonthlyBill>, BillRepository>();
            services.AddScoped<IRepository<PaymentRecord>, PaymentRepository>();
            services.AddScoped<IRepository<AuditLog>, AuditLogRepository>();
            services.AddScoped<IRepository<BillingRate>, BillingRateRepository>();
            services.AddScoped<IRepository<MeterReading>, MeterReadingRepository>();
            services.AddScoped<IRepository<NotificationHistory>, NotificationHistoryRepository>();
            services.AddScoped<IRepository<NotificationSettings>, NotificationSettingsRepository>();
            services.AddScoped<IRepository<NotificationMessage>, NotificationMessageRepository>();
            services.AddScoped<IRepository<PrintJob>, PrintJobRepository>();
            services.AddScoped<IRepository<BackupInfo>, BackupInfoRepository>();
            services.AddScoped<IRepository<BackupSchedule>, BackupScheduleRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBillingService, BillingService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<IPrintService, PrintService>();
            services.AddScoped<IBackupService, BackupService>();

            // Register CurrentUserService
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
            }
        }

        public string UserName
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            }
        }

        public string IpAddress
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }
        }

        public async Task<int> GetCurrentUserIdAsync()
        {
            var userId = UserId;
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User is not authenticated");
            return await Task.FromResult(userId.Value);
        }

        public async Task<string> GetCurrentUsernameAsync()
        {
            var username = UserName;
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("User is not authenticated");
            return await Task.FromResult(username);
        }

        public async Task<string> GetCurrentUserRoleAsync()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("role");
            if (roleClaim == null)
                throw new UnauthorizedAccessException("User role not found");
            return await Task.FromResult(roleClaim.Value);
        }

        public async Task<bool> HasPermissionAsync(string permission)
        {
            var permissions = _httpContextAccessor.HttpContext?.User?.FindFirst("permissions")?.Value?.Split(',');
            return await Task.FromResult(permissions?.Contains(permission) ?? false);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            return await Task.FromResult(_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false);
        }
    }
} 