namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class WithdrawalStartedEvent : AccountBalanceOperationEventBase
    {
        public WithdrawalStartedEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}