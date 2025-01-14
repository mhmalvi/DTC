namespace DTCBillingSystem.Core.Models.Enums
{
    public enum BackupType
    {
        Full,
        Differential,
        Incremental,
        Log,
        Schema,
        Data
    }

    public enum BackupFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Custom
    }
} 