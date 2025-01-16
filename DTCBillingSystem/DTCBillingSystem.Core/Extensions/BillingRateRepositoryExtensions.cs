using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Enums;

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