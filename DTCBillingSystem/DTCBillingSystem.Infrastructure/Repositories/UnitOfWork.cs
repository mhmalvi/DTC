using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DTCBillingSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private ICustomerRepository? _customers;
        private IBillingRateRepository? _billingRates;
        private IBillRepository? _bills;
        private IPaymentRepository? _payments;
        private IUserRepository? _users;
        private IAuditLogRepository? _auditLogs;
        private IMeterReadingRepository? _meterReadings;
        private INotificationHistoryRepository? _notificationHistory;
        private INotificationSettingsRepository? _notificationSettings;
        private INotificationMessageRepository? _notificationMessages;
        private IPrintJobRepository? _printJobs;
        private IBackupInfoRepository? _backupInfo;
        private IBackupScheduleRepository? _backupSchedules;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public IBillingRateRepository BillingRates => _billingRates ??= new BillingRateRepository(_context);
        public IBillRepository Bills => _bills ??= new BillRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        public IMeterReadingRepository MeterReadings => _meterReadings ??= new MeterReadingRepository(_context);
        public INotificationHistoryRepository NotificationHistory => _notificationHistory ??= new NotificationHistoryRepository(_context);
        public INotificationSettingsRepository NotificationSettings => _notificationSettings ??= new NotificationSettingsRepository(_context);
        public INotificationMessageRepository NotificationMessages => _notificationMessages ??= new NotificationMessageRepository(_context);
        public IPrintJobRepository PrintJobs => _printJobs ??= new PrintJobRepository(_context);
        public IBackupInfoRepository BackupInfo => _backupInfo ??= new BackupInfoRepository(_context);
        public IBackupScheduleRepository BackupSchedules => _backupSchedules ??= new BackupScheduleRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }
} 