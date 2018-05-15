namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class WithdrawalFailedEvent : AccountBalanceOperationEventBase
    {
        public WithdrawalFailedEvent(string clientId, string accountId, decimal amount, string operationId,
            string failReason)
            : base(clientId, accountId, amount, operationId, failReason)
        {
        }
    }
}