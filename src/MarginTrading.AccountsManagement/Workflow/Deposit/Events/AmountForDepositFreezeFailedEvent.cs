namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForDepositFreezeFailedEvent : AccountBalanceTransactionMessageBase
    {
        public AmountForDepositFreezeFailedEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}