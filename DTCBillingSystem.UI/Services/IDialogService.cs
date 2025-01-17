using System.Threading.Tasks;
using DTCBillingSystem.Core.Models.Entities;

namespace DTCBillingSystem.UI.Services
{
    public interface IDialogService
    {
        bool ShowConfirmation(string title, string message, string yesText = "Yes", string noText = "No");
        void ShowError(string title, string message);
        void ShowInformation(string title, string message);
        void ShowWarning(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No");
        Task ShowErrorAsync(string title, string message);
        Task ShowInformationAsync(string title, string message);
        Task ShowWarningAsync(string title, string message);
        Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
        Task ShowInfoAsync(string title, string message);
        Task<PaymentRecord?> ShowPaymentDialogAsync(MonthlyBill bill);
        Task ShowBillDetailsAsync(MonthlyBill bill);
    }
} 