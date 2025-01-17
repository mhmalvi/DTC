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
        private bool _isClosing = false;

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

                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");

                DataContext = _viewModel;
                Debug.WriteLine("DataContext set");

                // Subscribe to events
                Loaded += MainWindow_Loaded;
                ContentRendered += MainWindow_ContentRendered;
                Closing += MainWindow_Closing;
                Debug.WriteLine("Events subscribed");

                Debug.WriteLine("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in MainWindow constructor: {ex.Message}\nStack trace: {ex.StackTrace}";
                Debug.WriteLine($"CRITICAL ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainWindow_Loaded started");
            try
            {
                // Validate MainFrame
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

                // Ensure the frame is visible
                MainFrame.Visibility = Visibility.Visible;
                Debug.WriteLine("MainFrame visibility set to Visible");

                // Navigate to dashboard using the UI thread
                Debug.WriteLine("Attempting to navigate to DashboardView");
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Create and navigate to dashboard view
                        _navigationService.NavigateToAsync("DashboardView");
                        Debug.WriteLine("Navigation to DashboardView requested");

                        // Ensure window and frame are active and focused
                        Activate();
                        Focus();
                        MainFrame.Focus();
                        
                        // Force layout update
                        UpdateLayout();
                        MainFrame.UpdateLayout();
                        
                        Debug.WriteLine("Window and frame activated and focused");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error navigating to DashboardView: {ex.Message}");
                        _dialogService.ShowError("Navigation Error", $"Failed to navigate to dashboard: {ex.Message}");
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in MainWindow_Loaded: {ex.Message}\nStack trace: {ex.StackTrace}";
                Debug.WriteLine($"CRITICAL ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Debug.WriteLine("MainWindow_ContentRendered started");
            try
            {
                _viewModel.OnContentRendered();
                Debug.WriteLine("MainWindow_ContentRendered completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow_ContentRendered: {ex.Message}");
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("MainWindow_Closing started");
            try
            {
                if (!_isClosing)
                {
                    e.Cancel = true;
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow_Closing: {ex.Message}");
                e.Cancel = true;
            }
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
    }
} 