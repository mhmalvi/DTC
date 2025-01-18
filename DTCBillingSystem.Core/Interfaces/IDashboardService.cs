using DTCBillingSystem.Core.Models.DTOs;
using System.Threading.Tasks;

namespace DTCBillingSystem.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();
    }
} 