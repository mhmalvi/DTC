using System.Windows.Controls;
using DTCBillingSystem.UI.ViewModels;

namespace DTCBillingSystem.UI.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public SettingsView(SettingsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
} 