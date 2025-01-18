using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.Core.Models.Entities;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly IBillingService _billingService;

        public DialogService(IBillingService billingService)
        {
            _billingService = billingService;
        }

        public bool ShowConfirmation(string title, string message, string yesText = "Yes", string noText = "No")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInformation(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowWarning(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No")
        {
            return Task.FromResult(ShowConfirmation(title, message, yesText, noText));
        }

        public Task ShowErrorAsync(string title, string message)
        {
            ShowError(title, message);
            return Task.CompletedTask;
        }

        public Task ShowInformationAsync(string title, string message)
        {
            ShowInformation(title, message);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            ShowWarning(title, message);
            return Task.CompletedTask;
        }

        public Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
        {
            // TODO: Implement a proper input dialog
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
            var message = $@"Bill Details:
Customer ID: {bill.CustomerId}
Billing Date: {bill.BillingDate:d}
Previous Reading: {bill.PreviousReading:N2}
Current Reading: {bill.CurrentReading:N2}
Consumption: {bill.Consumption:N2}
Amount: {bill.Amount:C2}
Status: {(bill.IsPaid ? "Paid" : "Unpaid")}";

            return ShowInformationAsync("Bill Details", message);
        }

        public async Task<bool> ShowBillConfirmationDialog(MonthlyBill bill)
        {
            var message = $@"Please confirm the following bill details:

Customer ID: {bill.CustomerId}
Billing Date: {bill.BillingDate:d}
Previous Reading: {bill.PreviousReading:N2}
Current Reading: {bill.CurrentReading:N2}
Consumption: {bill.Consumption:N2}
Amount: {bill.Amount:C2}

Do you want to generate this bill?";

            var result = MessageBox.Show(message, "Confirm Bill Generation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _billingService.GenerateBillAsync(bill);
                    MessageBox.Show("Bill generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to generate bill: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return false;
        }

        public void ShowError(string message)
        {
            ShowError("Error", message);
        }

        public void ShowInformation(string message)
        {
            ShowInformation("Information", message);
        }

        public bool ShowConfirmation(string message)
        {
            return ShowConfirmation("Confirmation", message);
        }
    }
} 