using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IInvoiceRepository : IBaseRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetBillsByCustomerAsync(int customerId);
        Task<IEnumerable<Invoice>> GetOverdueBillsAsync();
    }
} 