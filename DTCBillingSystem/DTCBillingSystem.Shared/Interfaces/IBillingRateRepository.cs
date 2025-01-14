using System.Threading.Tasks;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Shared.Interfaces
{
    public interface IBillingRateRepository : IRepository<BillingRate>
    {
        Task<BillingRate> GetByCustomerTypeAsync(CustomerType customerType);
    }
} 