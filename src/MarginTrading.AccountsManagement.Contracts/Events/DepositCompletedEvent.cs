namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class DepositCompletedEvent : AccountBalanceOperationEventBase
    {
        public DepositCompletedEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}