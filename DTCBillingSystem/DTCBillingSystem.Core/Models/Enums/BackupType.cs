namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Types of backups
    /// </summary>
    public enum BackupType
    {
        Full,
        Incremental,
        Differential,
        Log,
        System,
        Data
    }
} 