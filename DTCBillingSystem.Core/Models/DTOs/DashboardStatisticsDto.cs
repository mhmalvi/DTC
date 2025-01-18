namespace DTCBillingSystem.Core.Models.DTOs
{
    public class DashboardStatisticsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalBillsThisMonth { get; set; }
        public int PaidBillsThisMonth { get; set; }
        public decimal TotalCollectionThisMonth { get; set; }
    }
} 