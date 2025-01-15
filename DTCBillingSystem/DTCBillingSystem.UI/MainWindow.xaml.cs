using System.Windows;
using DTCBillingSystem.UI.Services;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService, MainViewModel viewModel)
        {
            InitializeComponent();
            _navigationService = navigationService;
            DataContext = viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _navigationService.SetFrame(MainFrame);
            _navigationService.NavigateTo<DashboardViewModel>();
        }
    }
}
