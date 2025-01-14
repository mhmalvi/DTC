using System;
using System.Threading.Tasks;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        private IRepository<Customer> _customers;
        private IRepository<User> _users;
        private IRepository<MonthlyBill> _monthlyBills;
        private IRepository<PaymentRecord> _paymentRecords;
        private IRepository<PrintJob> _printJobs;
        private IRepository<BackupSchedule> _backupSchedules;
        private IRepository<BackupInfo> _backups;
        private IMeterReadingRepository _meterReadings;
        private IBillingRateRepository _billingRates;
        private IRepository<AuditLog> _auditLogs;
        private IRepository<NotificationMessage> _notifications;

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        public IRepository<Customer> Customers => _customers ??= new BaseRepository<Customer>(_context);
        public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);
        public IRepository<MonthlyBill> MonthlyBills => _monthlyBills ??= new BaseRepository<MonthlyBill>(_context);
        public IRepository<PaymentRecord> PaymentRecords => _paymentRecords ??= new BaseRepository<PaymentRecord>(_context);
        public IRepository<PrintJob> PrintJobs => _printJobs ??= new BaseRepository<PrintJob>(_context);
        public IRepository<BackupSchedule> BackupSchedules => _backupSchedules ??= new BaseRepository<BackupSchedule>(_context);
        public IRepository<BackupInfo> Backups => _backups ??= new BaseRepository<BackupInfo>(_context);
        public IMeterReadingRepository MeterReadings => _meterReadings ??= new MeterReadingRepository(_context);
        public IBillingRateRepository BillingRates => _billingRates ??= new BillingRateRepository(_context);
        public IRepository<AuditLog> AuditLogs => _auditLogs ??= new BaseRepository<AuditLog>(_context);
        public IRepository<NotificationMessage> Notifications => _notifications ??= new BaseRepository<NotificationMessage>(_context);

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