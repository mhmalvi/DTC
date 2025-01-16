using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DTCBillingSystem.UI.Views;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DialogService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return Task.FromResult(MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes);
        }

        public Task ShowInformationAsync(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return Task.CompletedTask;
        }

        public Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "")
        {
            var dialog = new InputDialog(message, title, defaultValue);
            return Task.FromResult(dialog.ShowDialog() == true ? dialog.InputText : null);
        }

        public Task ShowInfoAsync(string title, string message)
        {
            return ShowInformationAsync(title, message);
        }

        public Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<PaymentDialog>();
            dialog.DataContext = bill;
            return Task.FromResult(dialog.ShowDialog() == true ? (dialog.DataContext as PaymentRecord) : null);
        }

        public Task ShowBillDetailsAsync(MonthlyBill bill)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<BillDetailsDialog>();
            dialog.DataContext = bill;
            dialog.ShowDialog();
            return Task.CompletedTask;
        }
    }

    internal class InputDialog : Window
    {
        private readonly System.Windows.Controls.TextBox _textBox;
        private readonly System.Windows.Controls.Button _okButton;
        private readonly System.Windows.Controls.Button _cancelButton;

        public string InputText => _textBox.Text;

        public InputDialog(string message, string title, string defaultValue)
        {
            Title = title;
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            var messageLabel = new System.Windows.Controls.Label
            {
                Content = message,
                Margin = new Thickness(10)
            };
            System.Windows.Controls.Grid.SetRow(messageLabel, 0);
            grid.Children.Add(messageLabel);

            _textBox = new System.Windows.Controls.TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(10)
            };
            System.Windows.Controls.Grid.SetRow(_textBox, 1);
            grid.Children.Add(_textBox);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            _okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 75,
                Height = 23,
                Margin = new Thickness(0, 0, 10, 0)
            };
            _okButton.Click += (s, e) => { DialogResult = true; Close(); };

            _cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 23
            };
            _cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(_okButton);
            buttonPanel.Children.Add(_cancelButton);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
} 