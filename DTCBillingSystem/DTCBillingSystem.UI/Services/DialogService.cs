using System.Threading.Tasks;
using System.Windows;

namespace DTCBillingSystem.UI.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowErrorAsync(string title, string message)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }).Task;
        }

        public Task ShowInfoAsync(string title, string message)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }).Task;
        }

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return result == MessageBoxResult.Yes;
            }).Task;
        }

        public Task<string> ShowInputAsync(string title, string message, string defaultValue = "")
        {
            // TODO: Implement custom input dialog
            // For now, return empty string
            return Task.FromResult(string.Empty);
        }
    }
} 