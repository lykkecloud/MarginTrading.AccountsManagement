namespace MarginTrading.AccountsManagement.Contracts.Messages
{
    /// <summary>
    /// What happend to the account
    /// </summary>
    public enum AccountChangedEventType
    {
        /// <summary>
        /// Well, the account was created
        /// </summary>
        Created = 1,
        
        /// <summary>
        /// Arbitrary account field was updated
        /// </summary>
        Updated = 2,
        
        /// <summary>
        /// The balance was updated
        /// </summary>
        BalanceUpdated = 4,
    }
}