using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        private ICustomerRepository? _customers;
        private IMonthlyBillRepository? _monthlyBills;
        private IPaymentRecordRepository? _paymentRecords;
        private IUserRepository? _users;
        private IMeterReadingRepository? _meterReadings;
        private IPrintJobRepository? _printJobs;
        private IAuditLogRepository? _auditLogs;
        private IBackupInfoRepository? _backupInfos;
        private IBackupScheduleRepository? _backupSchedules;

        public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
        public IMonthlyBillRepository MonthlyBills => _monthlyBills ??= new MonthlyBillRepository(_context);
        public IPaymentRecordRepository PaymentRecords => _paymentRecords ??= new PaymentRecordRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IMeterReadingRepository MeterReadings => _meterReadings ??= new MeterReadingRepository(_context);
        public IPrintJobRepository PrintJobs => _printJobs ??= new PrintJobRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        public IBackupInfoRepository BackupInfos => _backupInfos ??= new BackupInfoRepository(_context);
        public IBackupScheduleRepository BackupSchedules => _backupSchedules ??= new BackupScheduleRepository(_context);

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("A concurrency error occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 