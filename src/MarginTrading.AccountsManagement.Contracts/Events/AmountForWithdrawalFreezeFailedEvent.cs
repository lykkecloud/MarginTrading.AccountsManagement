namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFreezeFailedEvent : AccountBalanceMessageBase
    {
        public AmountForWithdrawalFreezeFailedEvent(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}