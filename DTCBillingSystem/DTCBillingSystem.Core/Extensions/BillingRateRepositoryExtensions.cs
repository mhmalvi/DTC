using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Shared.Models.Entities;
using DTCBillingSystem.Shared.Interfaces;
using DTCBillingSystem.Shared.Models.Enums;

namespace DTCBillingSystem.Core.Extensions
{
    public static class BillingRateRepositoryExtensions
    {
        public static async Task<BillingRate?> GetByCustomerTypeAsync(
            this IRepository<BillingRate> repository,
            CustomerType customerType)
        {
            var rates = await repository.FindAsync(x => x.CustomerType == customerType);
            return rates.FirstOrDefault();
        }
    }
} 