using System.Windows.Controls;

namespace DTCBillingSystem.UI.Views
{
    public partial class CustomersView : UserControl
    {
        public CustomersView()
        {
            InitializeComponent();
            DataContext = new ViewModels.CustomersViewModel();
        }
    }
} 