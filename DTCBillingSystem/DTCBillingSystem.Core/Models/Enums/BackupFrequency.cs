namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the frequency of automated backups
    /// </summary>
    public enum BackupFrequency
    {
        /// <summary>
        /// Run backup daily
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Run backup weekly
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Run backup monthly
        /// </summary>
        Monthly = 2,

        /// <summary>
        /// Run backup quarterly
        /// </summary>
        Quarterly = 3,

        /// <summary>
        /// Run backup yearly
        /// </summary>
        Yearly = 4,

        /// <summary>
        /// Run backup custom
        /// </summary>
        Custom = 5
    }
} 