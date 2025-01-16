using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using System.Diagnostics;

namespace DTCBillingSystem.UI.Views
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly MainViewModel _viewModel;

        public MainWindow(INavigationService navigationService, IDialogService dialogService, MainViewModel viewModel)
        {
            Debug.WriteLine("MainWindow constructor started");
            try
            {
                // Store services
                _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
                Debug.WriteLine("Navigation service initialized");
                
                _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
                Debug.WriteLine("Dialog service initialized");
                
                _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
                Debug.WriteLine("ViewModel initialized");

                // Initialize window
                Debug.WriteLine("Starting InitializeComponent");
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                // Set DataContext first
                DataContext = _viewModel;
                Debug.WriteLine("DataContext set to MainViewModel");

                // Validate MainFrame exists
                if (MainFrame == null)
                {
                    Debug.WriteLine("ERROR: MainFrame is null after initialization");
                    throw new InvalidOperationException("MainFrame control not found");
                }
                Debug.WriteLine("MainFrame validated successfully");

                // Set window properties
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
                ResizeMode = ResizeMode.CanResize;
                Debug.WriteLine("Window properties set");

                // Add event handlers
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
                ContentRendered += MainWindow_ContentRendered;
                Debug.WriteLine("Event handlers attached");
            }
            catch (Exception ex)
            {
                var errorMsg = $"MainWindow initialization error: {ex.Message}\nStack trace: {ex.StackTrace}";
                Debug.WriteLine(errorMsg);
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
            Debug.WriteLine("MainWindow_Loaded event started");
            try
            {
                // Validate MainFrame exists
                if (MainFrame == null)
                {
                    Debug.WriteLine("ERROR: MainFrame is null in Loaded event");
                    throw new InvalidOperationException("MainFrame control not found");
                }
                Debug.WriteLine("MainFrame validated in Loaded event");

                // Initialize navigation service with the frame
                Debug.WriteLine("Initializing navigation service with MainFrame");
                _navigationService.Initialize(MainFrame, this);
                Debug.WriteLine("Navigation service initialized successfully");
                
                // Navigate to dashboard
                Debug.WriteLine("Attempting to navigate to DashboardView");
                _navigationService.NavigateToAsync("DashboardView");
                Debug.WriteLine("Navigation to DashboardView requested");
                
                Activate();
                Focus();
                Debug.WriteLine("Window activated and focused");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in MainWindow_Loaded: {ex.Message}\nStack trace: {ex.StackTrace}";
                Debug.WriteLine(errorMsg);
                MessageBox.Show(
                    $"Failed to initialize window: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow_ContentRendered event fired");
            try
            {
                if (MainFrame == null)
                {
                    Debug.WriteLine("ERROR: MainFrame is null in ContentRendered event");
                    return;
                }
                Debug.WriteLine($"MainFrame status - Content: {MainFrame.Content}, NavigationService: {MainFrame.NavigationService != null}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ContentRendered: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("MainWindow_Closing event started");
            try
            {
                // Add any cleanup logic here
                Debug.WriteLine("MainWindow closing successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during window closing: {ex.Message}");
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