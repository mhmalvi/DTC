using System.Threading.Tasks;
using System.Windows;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No")
        {
            return await Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    return result == MessageBoxResult.Yes;
                });
            });
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            });
        }

        public async Task ShowInformationAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            });
        }

        public async Task ShowWarningAsync(string title, string message)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
            });
        }

        public async Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
        {
            // TODO: Implement a proper input dialog
            // For now, return null to indicate cancellation
            return await Task.FromResult<string?>(null);
        }

        public async Task<bool?> ShowDialogAsync(Window dialog)
        {
            return await Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    return dialog.ShowDialog();
                });
            });
        }

        public async Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill)
        {
            // TODO: Implement payment dialog
            return await Task.FromResult<PaymentRecord?>(null);
        }

        public async Task ShowBillDetailsAsync(MonthlyBill bill)
        {
            var message = $"Bill Details:\n" +
                         $"Bill Number: {bill.BillNumber}\n" +
                         $"Customer: {bill.Customer?.Name}\n" +
                         $"Amount: {bill.Amount:C}\n" +
                         $"Status: {bill.Status}\n" +
                         $"Due Date: {bill.DueDate:d}";

            await ShowInformationAsync("Bill Details", message);
        }
    }
} 