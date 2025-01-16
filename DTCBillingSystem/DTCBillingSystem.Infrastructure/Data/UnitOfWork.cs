using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public IUserRepository Users { get; }
        public ICustomerRepository Customers { get; }
        public IMonthlyBillRepository MonthlyBills { get; }
        public IPaymentRecordRepository PaymentRecords { get; }
        public IBackupInfoRepository BackupInfos { get; }
        public IPrintJobRepository PrintJobs { get; }
        public IMeterReadingRepository MeterReadings { get; }
        public IBackupScheduleRepository BackupSchedules { get; }
        public IBillingRateRepository BillingRates { get; }
        public IAuditLogRepository AuditLogs { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IUserRepository userRepository,
            ICustomerRepository customerRepository,
            IMonthlyBillRepository monthlyBillRepository,
            IPaymentRecordRepository paymentRecordRepository,
            IBackupInfoRepository backupInfoRepository,
            IPrintJobRepository printJobRepository,
            IMeterReadingRepository meterReadingRepository,
            IBackupScheduleRepository backupScheduleRepository,
            IBillingRateRepository billingRateRepository,
            IAuditLogRepository auditLogRepository)
        {
            _context = context;
            Users = userRepository;
            Customers = customerRepository;
            MonthlyBills = monthlyBillRepository;
            PaymentRecords = paymentRecordRepository;
            BackupInfos = backupInfoRepository;
            PrintJobs = printJobRepository;
            MeterReadings = meterReadingRepository;
            BackupSchedules = backupScheduleRepository;
            BillingRates = billingRateRepository;
            AuditLogs = auditLogRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Concurrency conflict detected while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();

                if (_transaction == null)
                    throw new InvalidOperationException("No transaction to commit");

                await _transaction.CommitAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction == null)
                    throw new InvalidOperationException("No transaction to rollback");

                await _transaction.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
} 