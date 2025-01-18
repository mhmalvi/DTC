using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DTCBillingSystem.Core.Models.DTOs;

namespace DTCBillingSystem.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            var totalCustomers = await _unitOfWork.Customers.CountAsync();
            var totalActiveCustomers = await _unitOfWork.Customers.CountAsync(c => c.Status == Core.Models.Enums.CustomerStatus.Active);

            var currentMonth = DateTime.Now.Date.AddDays(1 - DateTime.Now.Day);
            var monthlyBills = await _unitOfWork.MonthlyBills.GetAllAsync();
            var billsThisMonth = monthlyBills.Where(b => b.BillingDate.Year == currentMonth.Year && b.BillingDate.Month == currentMonth.Month);

            var totalBillsThisMonth = billsThisMonth.Count();
            var totalPaidBillsThisMonth = billsThisMonth.Count(b => b.IsPaid);
            var totalCollectionThisMonth = billsThisMonth.Where(b => b.IsPaid).Sum(b => b.Amount);

            return new DashboardStatisticsDto
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = totalActiveCustomers,
                TotalBillsThisMonth = totalBillsThisMonth,
                PaidBillsThisMonth = totalPaidBillsThisMonth,
                TotalCollectionThisMonth = totalCollectionThisMonth
            };
        }
    }
} 