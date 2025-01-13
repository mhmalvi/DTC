using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Generic repository interface for CRUD operations
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Find entities by predicate
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Update existing entity
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Get entities with pagination
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, Expression<Func<T, bool>> predicate = null);

        /// <summary>
        /// Check if any entity matches the predicate
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count entities matching the predicate
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
    }
} 