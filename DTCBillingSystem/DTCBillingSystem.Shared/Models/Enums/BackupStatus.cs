namespace DTCBillingSystem.Shared.Models.Enums
{
    /// <summary>
    /// Represents the status of a backup operation
    /// </summary>
    public enum BackupStatus
    {
        /// <summary>
        /// Backup is pending or not started
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Backup is in progress
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Backup completed successfully
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Backup failed
        /// </summary>
        Failed = 3
    }
} 