using System;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Interface for managing database transactions and repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Repository for customers
        /// </summary>
        IRepository<Customer> Customers { get; }

        /// <summary>
        /// Repository for billing rates
        /// </summary>
        IRepository<BillingRate> BillingRates { get; }

        /// <summary>
        /// Repository for monthly bills
        /// </summary>
        IRepository<MonthlyBill> MonthlyBills { get; }

        /// <summary>
        /// Repository for payment records
        /// </summary>
        IRepository<PaymentRecord> PaymentRecords { get; }

        /// <summary>
        /// Repository for users
        /// </summary>
        IRepository<User> Users { get; }

        /// <summary>
        /// Repository for audit logs
        /// </summary>
        IRepository<AuditLog> AuditLogs { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begins a new transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
} 