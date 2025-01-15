using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing meter readings
    /// </summary>
    public interface IMeterReadingService
    {
        /// <summary>
        /// Creates a new meter reading
        /// </summary>
        Task<MeterReading> CreateReadingAsync(MeterReading reading, int userId);

        /// <summary>
        /// Updates an existing meter reading
        /// </summary>
        Task<MeterReading> UpdateReadingAsync(int readingId, MeterReading reading, int userId);

        /// <summary>
        /// Gets all readings for a specific customer
        /// </summary>
        Task<IQueryable<MeterReading>> GetReadingsForCustomerAsync(int customerId);

        /// <summary>
        /// Gets the latest reading for a specific customer
        /// </summary>
        Task<MeterReading> GetLatestReadingForCustomerAsync(int customerId);

        /// <summary>
        /// Deletes a meter reading
        /// </summary>
        Task DeleteReadingAsync(int readingId, int userId);
    }
} 