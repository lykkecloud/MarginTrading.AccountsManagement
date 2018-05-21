namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class DepositStartedEvent : AccountBalanceTransactionMessageBase
    {
        public DepositStartedEvent(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}