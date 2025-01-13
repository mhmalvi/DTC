using System;
using System.Windows.Input;
using DTCBillingSystem.Core.Interfaces;
using DTCBillingSystem.UI.Commands;

namespace DTCBillingSystem.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;
        private ViewModelBase _currentViewModel;
        private string _currentUserName;

        public MainViewModel(IUserService userService, IAuditService auditService)
        {
            _userService = userService;
            _auditService = auditService;
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public ICommand LogoutCommand { get; }

        public void NavigateToView(string viewName)
        {
            ViewModelBase newViewModel = viewName switch
            {
                "Dashboard" => new DashboardViewModel(),
                "Customers" => new CustomerViewModel(),
                "Bills" => new BillViewModel(),
                "Payments" => new PaymentViewModel(),
                "Reports" => new ReportViewModel(),
                "Settings" => new SettingsViewModel(),
                _ => throw new ArgumentException($"View {viewName} not found")
            };

            CurrentViewModel = newViewModel;
        }

        private void ExecuteLogout(object parameter)
        {
            // TODO: Clear user session
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            System.Windows.Application.Current.MainWindow.Close();
        }
    }
} 