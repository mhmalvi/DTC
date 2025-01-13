using System.Windows;

namespace DTCBillingSystem.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MenuListBox.SelectionChanged += MenuListBox_SelectionChanged;
        }

        private void MenuListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                var selectedItem = MenuListBox.SelectedItem as System.Windows.Controls.ListBoxItem;
                if (selectedItem != null)
                {
                    var menuText = (selectedItem.Content as System.Windows.Controls.StackPanel)
                        ?.Children[1] as System.Windows.Controls.TextBlock;
                    
                    if (menuText != null)
                    {
                        viewModel.NavigateToView(menuText.Text);
                    }
                }
            }
        }
    }
} 