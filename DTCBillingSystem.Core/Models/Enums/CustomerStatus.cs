namespace DTCBillingSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the current status of a customer in the system
    /// </summary>
    public enum CustomerStatus
    {
        /// <summary>
        /// Customer is active and in good standing
        /// </summary>
        Active = 0,

        /// <summary>
        /// Customer is temporarily inactive
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Customer account is suspended
        /// </summary>
        Suspended = 2,

        /// <summary>
        /// Customer account is terminated
        /// </summary>
        Terminated = 3,

        /// <summary>
        /// Customer is pending activation
        /// </summary>
        PendingActivation = 4,

        /// <summary>
        /// Customer has defaulted on payments
        /// </summary>
        Defaulted = 5
    }
} 