namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the frequency of automated backups
    /// </summary>
    public enum BackupFrequency
    {
        /// <summary>
        /// Run backup once
        /// </summary>
        Once,

        /// <summary>
        /// Run backup hourly
        /// </summary>
        Hourly,

        /// <summary>
        /// Run backup daily
        /// </summary>
        Daily,

        /// <summary>
        /// Run backup weekly
        /// </summary>
        Weekly,

        /// <summary>
        /// Run backup monthly
        /// </summary>
        Monthly,

        /// <summary>
        /// Run backup quarterly
        /// </summary>
        Quarterly,

        /// <summary>
        /// Run backup yearly
        /// </summary>
        Yearly
    }
} 