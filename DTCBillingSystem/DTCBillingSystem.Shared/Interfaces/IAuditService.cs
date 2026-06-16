using System;
using System.Threading.Tasks;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IAuditService
    {
        Task LogCreateAsync<T>(T entity, Guid userId, string? notes = null) where T : class;
        Task LogUpdateAsync<T>(T entity, Guid userId, string? notes = null) where T : class;
        Task LogDeleteAsync<T>(T entity, Guid userId, string? notes = null) where T : class;
    }
} 