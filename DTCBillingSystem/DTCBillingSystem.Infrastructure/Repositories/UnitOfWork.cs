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

        private IRepository<Customer>? _customers;
        private IRepository<BillingRate>? _billingRates;
        private IRepository<MonthlyBill>? _bills;
        private IRepository<PaymentRecord>? _payments;
        private IRepository<User>? _users;
        private IRepository<AuditLog>? _auditLogs;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Customer> Customers => _customers ??= new BaseRepository<Customer>(_context);
        public IRepository<BillingRate> BillingRates => _billingRates ??= new BaseRepository<BillingRate>(_context);
        public IRepository<MonthlyBill> Bills => _bills ??= new BaseRepository<MonthlyBill>(_context);
        public IRepository<PaymentRecord> Payments => _payments ??= new BaseRepository<PaymentRecord>(_context);
        public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);
        public IRepository<AuditLog> AuditLogs => _auditLogs ??= new BaseRepository<AuditLog>(_context);

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