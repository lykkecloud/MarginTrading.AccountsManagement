namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Abject with additional information for activities
    /// </summary>
    public class AccountChangeMetadata
    {
        /// <summary>
        /// Account state before action, that generated event
        /// </summary>
        public AccountContract PreviousAccountSnapshot { get; set; }
    }
}