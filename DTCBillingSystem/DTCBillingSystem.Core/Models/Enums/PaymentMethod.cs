namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents different payment methods
    /// </summary>
    public enum PaymentMethod
    {
        Cash = 0,
        CreditCard = 1,
        DebitCard = 2,
        BankTransfer = 3,
        Check = 4,
        OnlineBanking = 5,
        MobileWallet = 6,
        Other = 99
    }
} 