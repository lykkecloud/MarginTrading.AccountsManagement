namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class AccountChargeManuallyRequest
    {
        /// <summary>
        /// The amount of money to add to the account's balance 
        /// </summary>
        public decimal AmountDelta { get; set; }
        
        /// <summary>
        /// Reason of modification
        /// </summary>
        public string Reason { get; set; }
    }
}