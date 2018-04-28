namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class DepositFailedEvent : AccountBalanceOperationEventBase
    {
        public DepositFailedEvent(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}