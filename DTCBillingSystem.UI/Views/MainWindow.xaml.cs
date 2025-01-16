using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly MainViewModel _viewModel;

        public MainWindow(INavigationService navigationService, IDialogService dialogService, MainViewModel viewModel)
        {
            try
            {
                InitializeComponent();
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
                _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                DataContext = _viewModel;

                // Initialize navigation service with the main frame
                _navigationService.Initialize(MainFrame, this);

                // Navigate to dashboard by default
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        _navigationService.NavigateToAsync("DashboardView");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Failed to load dashboard: {ex.Message}\n\nThe application will continue to work, but some features may be limited.",
                            "Navigation Warning",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }));

                // Handle window closing
                Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error initializing main window: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ManageCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToAsync("Customers");
        }

        private async void ImportCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("Import Customers", "This feature is coming soon!");
        }

        private async void ExportCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("Export Customers", "This feature is coming soon!");
        }

        private async void GenerateBillsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("Generate Bills", "This feature is coming soon!");
        }

        private async void ProcessPaymentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("Process Payments", "This feature is coming soon!");
        }

        private async void ViewReportsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("View Reports", "This feature is coming soon!");
        }

        private async void UserManagementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("User Management", "This feature is coming soon!");
        }

        private async void SystemSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("System Settings", "This feature is coming soon!");
        }

        private async void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await _dialogService.ShowInfoAsync("About", 
                "DTC Billing System\nVersion 1.0\n\n© 2024 Your Company");
        }
    }
} 