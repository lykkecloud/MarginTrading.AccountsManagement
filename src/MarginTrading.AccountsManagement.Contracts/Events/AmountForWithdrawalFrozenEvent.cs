namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForWithdrawalFrozenEvent : AccountBalanceBaseEvent
    {
        public AmountForWithdrawalFrozenEvent(string clientId, string accountId, decimal amount, string operationId, string reason)
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}