using System.Threading.Tasks;
using System.Windows;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowInformationAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.CompletedTask;
        }

        public Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
        {
            // TODO: Implement a proper input dialog
            // For now, return null to indicate cancellation
            return Task.FromResult<string?>(null);
        }

        public Task ShowInfoAsync(string title, string message)
        {
            return ShowInformationAsync(title, message);
        }

        public Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill)
        {
            // TODO: Implement payment dialog
            return Task.FromResult<PaymentRecord?>(null);
        }

        public Task ShowBillDetailsAsync(MonthlyBill bill)
        {
            // TODO: Implement bill details dialog
            var message = $"Bill Details:\n" +
                         $"Bill Number: {bill.BillNumber}\n" +
                         $"Customer: {bill.Customer?.Name}\n" +
                         $"Amount: {bill.Amount:C}\n" +
                         $"Status: {bill.Status}\n" +
                         $"Due Date: {bill.DueDate:d}";

            return ShowInformationAsync("Bill Details", message);
        }
    }
} 