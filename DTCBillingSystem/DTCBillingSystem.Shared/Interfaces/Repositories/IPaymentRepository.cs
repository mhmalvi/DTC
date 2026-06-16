using DTCBillingSystem.Shared.Models.Entities;

namespace DTCBillingSystem.Shared.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepository<PaymentRecord>
    {
        // Add any payment-specific repository methods here
    }
} 