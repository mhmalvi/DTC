using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models;
using DTCBillingSystem.Infrastructure.Data.Repositories;

namespace DTCBillingSystem.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        private IRepository<Customer> _customers;
        private IRepository<BillingRate> _billingRates;
        private IRepository<MonthlyBill> _monthlyBills;
        private IRepository<PaymentRecord> _paymentRecords;
        private IRepository<User> _users;
        private IRepository<AuditLog> _auditLogs;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Customer> Customers => _customers ??= new BaseRepository<Customer>(_context);
        public IRepository<BillingRate> BillingRates => _billingRates ??= new BaseRepository<BillingRate>(_context);
        public IRepository<MonthlyBill> MonthlyBills => _monthlyBills ??= new BaseRepository<MonthlyBill>(_context);
        public IRepository<PaymentRecord> PaymentRecords => _paymentRecords ??= new BaseRepository<PaymentRecord>(_context);
        public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);
        public IRepository<AuditLog> AuditLogs => _auditLogs ??= new BaseRepository<AuditLog>(_context);

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("A concurrency error occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while saving changes to the database.", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction?.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
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

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction?.RollbackAsync();
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
            if (!_disposed && disposing)
            {
                _context.Dispose();
                _transaction?.Dispose();
            }
            _disposed = true;
        }
    }
} 