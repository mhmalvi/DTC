using System;
using System.ComponentModel;
using System.Windows;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DTCBillingSystem.UI.Views
{
    public partial class MainWindow : ScopedWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public MainWindow(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            try
            {
                InitializeComponent();

                // Get services from the scope
                _viewModel = ServiceScope.ServiceProvider.GetRequiredService<MainViewModel>();
                _navigationService = ServiceScope.ServiceProvider.GetRequiredService<INavigationService>();
                _dialogService = ServiceScope.ServiceProvider.GetRequiredService<IDialogService>();

                if (MainFrame == null)
                {
                    throw new InvalidOperationException("MainFrame not found in XAML");
                }

                // Set DataContext before initialization
                DataContext = _viewModel;

                // Subscribe to window events
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow constructor: {ex}");
                MessageBox.Show($"Error initializing MainWindow: {ex.Message}\n\nDetails: {ex}",
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
                Debug.WriteLine("MainWindow_Loaded called");
                
                // Initialize navigation service
                if (MainFrame != null && _navigationService != null)
                {
                    Debug.WriteLine("Initializing navigation service in MainWindow");
                    _navigationService.Initialize(MainFrame, this);
                    
                    // Navigate to dashboard
                    Debug.WriteLine("Navigating to dashboard from MainWindow_Loaded");
                    _navigationService.NavigateToDashboard();
                    Debug.WriteLine("Dashboard navigation completed in MainWindow_Loaded");
                }
                else
                {
                    Debug.WriteLine("MainFrame or NavigationService is null in MainWindow_Loaded");
                    throw new InvalidOperationException("MainFrame or NavigationService is null");
                }
                
                _viewModel.OnContentRendered();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow_Loaded: {ex}");
                MessageBox.Show($"Error initializing main window: {ex.Message}",
                              "Initialization Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                _viewModel.OnClosing();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow_Closing: {ex}");
            }
        }
    }
} 