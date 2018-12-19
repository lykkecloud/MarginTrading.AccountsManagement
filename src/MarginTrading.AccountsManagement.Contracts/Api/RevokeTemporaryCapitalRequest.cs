namespace MarginTrading.AccountsManagement.Contracts.Api
{
    /// <summary>
    /// Request to revoke temporary capital previously given to the account
    /// </summary>
    public class RevokeTemporaryCapitalRequest
    {
        /// <summary>
        /// Unique external identifier of the temporary capital change entry (e.g., TCC + 10 alphanumeric characters)
        /// </summary>
        public string EventSourceId { get; set; }
        
        /// <summary>
        /// User account ID
        /// </summary>
        public string AccountId { get; set; }
        
        /// <summary>
        /// Event source ID that must be revoked. Optional.
        /// </summary>
        public string RevokeEventSourceId { get; set; }
        
        /// <summary>
        /// User who requested revocation
        /// </summary>
        public string RevokedBy { get; set; }
    }
}