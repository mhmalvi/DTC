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
                // Store services
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
                _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

                // Initialize window
                InitializeComponent();

                // Set DataContext first
                DataContext = _viewModel;

                // Validate MainFrame exists
                if (MainFrame == null)
                {
                    throw new InvalidOperationException("MainFrame control not found");
                }

                // Set window properties
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                ResizeMode = ResizeMode.CanResize;

                // Add event handlers
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                ContentRendered += MainWindow_ContentRendered;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow initialization error: {ex.Message}\nStack trace: {ex.StackTrace}");
                MessageBox.Show(
                    $"Failed to initialize main window: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Window loaded");
                Activate();
                Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainWindow_Loaded: {ex.Message}");
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MainWindow: Content rendered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainWindow_ContentRendered: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to exit the application?",
                    "Exit Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                // Ensure proper cleanup
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during window closing: {ex.Message}");
                Application.Current.Shutdown();
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ManageCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToAsync("CustomerView");
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
            await _dialogService.ShowInfoAsync("About", "DTC Billing System\nVersion 1.0\n© 2024 Aethon");
        }
    }
} 