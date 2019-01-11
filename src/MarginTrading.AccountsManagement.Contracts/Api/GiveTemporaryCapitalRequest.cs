namespace MarginTrading.AccountsManagement.Contracts.Api
{
    /// <summary>
    /// Request to start temporary capital charge to the account
    /// </summary>
    public class GiveTemporaryCapitalRequest
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
        /// Amount in settlement currency
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Reason for the temporary capital change
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Additional approval comments of support staff who approved the TCC
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// Any external additional info
        /// </summary>
        public string AdditionalInfo { get; set; }
    }
}