using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private bool _isClosing = false;

        public MainWindow(INavigationService navigationService, IDialogService dialogService, MainViewModel viewModel)
        {
            Debug.WriteLine("MainWindow constructor started");
            
            // Store dependencies first
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            Debug.WriteLine("Dependencies stored");

            try
            {
                // Set DataContext before component initialization
                DataContext = _viewModel;
                Debug.WriteLine("DataContext set");

                // Initialize the window components
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                // Initialize navigation in the Loaded event
                Loaded += MainWindow_Loaded;
                ContentRendered += MainWindow_ContentRendered;
                Closing += MainWindow_Closing;
                Debug.WriteLine("Events subscribed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow constructor: {ex}");
                MessageBox.Show($"Error initializing main window: {ex.Message}", "Initialization Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainFrame == null)
                {
                    throw new InvalidOperationException("MainFrame control not found");
                }
                _navigationService.Initialize(MainFrame, this);
                Debug.WriteLine("Navigation service initialized with frame");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow_Loaded: {ex}");
                MessageBox.Show($"Error initializing navigation: {ex.Message}", "Navigation Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow content rendered");
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isClosing)
            {
                e.Cancel = true;
                _isClosing = true;
                Application.Current.Shutdown();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ExitMenuItem_Click started");
            try
            {
                var result = _dialogService.ShowConfirmation(
                    "Exit Application",
                    "Are you sure you want to exit the application?",
                    "Yes",
                    "No");

                if (result)
                {
                    _isClosing = true;
                    Debug.WriteLine("User confirmed exit, shutting down application");
                    Application.Current.Shutdown();
                }
                else
                {
                    Debug.WriteLine("User cancelled exit");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ExitMenuItem_Click: {ex.Message}");
                _dialogService.ShowError("Error", $"Failed to exit application: {ex.Message}");
            }
        }

        private void ManageCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ManageCustomersMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("CustomerView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to CustomerView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Customer Management: {ex.Message}");
            }
        }

        private void ImportCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ImportCustomersMenuItem_Click - Not implemented");
            _dialogService.ShowInformation("Not Implemented", "This feature is not yet implemented.");
        }

        private void ExportCustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ExportCustomersMenuItem_Click - Not implemented");
            _dialogService.ShowInformation("Not Implemented", "This feature is not yet implemented.");
        }

        private void GenerateBillsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("GenerateBillsMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("BillView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to BillView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Bill Generation: {ex.Message}");
            }
        }

        private void ProcessPaymentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ProcessPaymentsMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("PaymentView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to PaymentView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Payment Processing: {ex.Message}");
            }
        }

        private void ViewReportsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ViewReportsMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("ReportView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to ReportView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Reports: {ex.Message}");
            }
        }

        private void UserManagementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("UserManagementMenuItem_Click - Not implemented");
            _dialogService.ShowInformation("Not Implemented", "This feature is not yet implemented.");
        }

        private void SystemSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SystemSettingsMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("SettingsView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to SettingsView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Settings: {ex.Message}");
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("AboutMenuItem_Click started");
            _dialogService.ShowInformation(
                "About DTC Billing System",
                "DTC Billing System\nVersion 1.0\n\n© 2024 Your Company");
        }

        private void DashboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DashboardMenuItem_Click started");
            try
            {
                _navigationService.NavigateToAsync("DashboardView");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to DashboardView: {ex.Message}");
                _dialogService.ShowError("Navigation Error", $"Failed to navigate to Dashboard: {ex.Message}");
            }
        }
    }
} 