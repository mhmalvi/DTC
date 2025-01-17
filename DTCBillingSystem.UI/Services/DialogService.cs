using System;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        public bool ShowConfirmation(string title, string message, string yesText = "Yes", string noText = "No")
        {
            Debug.WriteLine($"Showing confirmation dialog: {title} - {message}");
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            return result == MessageBoxResult.Yes;
        }

        public void ShowError(string title, string message)
        {
            Debug.WriteLine($"Showing error dialog: {title} - {message}");
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void ShowInformation(string title, string message)
        {
            Debug.WriteLine($"Showing information dialog: {title} - {message}");
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowWarning(string title, string message)
        {
            Debug.WriteLine($"Showing warning dialog: {title} - {message}");
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No")
        {
            Debug.WriteLine($"Showing async confirmation dialog: {title} - {message}");
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);
                return result == MessageBoxResult.Yes;
            }).Task;
        }

        public Task ShowErrorAsync(string title, string message)
        {
            Debug.WriteLine($"Showing async error dialog: {title} - {message}");
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }).Task;
        }

        public Task ShowInformationAsync(string title, string message)
        {
            Debug.WriteLine($"Showing async information dialog: {title} - {message}");
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }).Task;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            Debug.WriteLine($"Showing async warning dialog: {title} - {message}");
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }).Task;
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