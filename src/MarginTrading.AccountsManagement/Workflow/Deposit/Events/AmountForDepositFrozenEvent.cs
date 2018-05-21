namespace MarginTrading.AccountsManagement.Contracts.Events
{
    public class AmountForDepositFrozenEvent : AccountBalanceTransactionMessageBase
    {
        public AmountForDepositFrozenEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason) : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}