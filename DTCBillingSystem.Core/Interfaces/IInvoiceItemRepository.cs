using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IInvoiceItemRepository : IRepository<InvoiceItem>
    {
        Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(int invoiceId);
        Task<decimal> GetTotalAmountByInvoiceIdAsync(int invoiceId);
    }
} 