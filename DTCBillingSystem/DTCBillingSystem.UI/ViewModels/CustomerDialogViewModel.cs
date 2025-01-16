using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DTCBillingSystem.Core.Models.Entities;
using DTCBillingSystem.Core.Models.Enums;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class CustomerDialogViewModel : ViewModelBase
    {
        private Customer _customer;
        private bool _isNew;

        public CustomerDialogViewModel()
        {
            _customer = new Customer
            {
                CreatedAt = DateTime.UtcNow,
                Status = CustomerStatus.Active,
                CustomerType = CustomerType.Residential
            };
            _isNew = true;
        }

        public CustomerDialogViewModel(Customer customer)
        {
            _customer = customer;
            _isNew = false;
        }

        // Properties and commands...
    }
} 