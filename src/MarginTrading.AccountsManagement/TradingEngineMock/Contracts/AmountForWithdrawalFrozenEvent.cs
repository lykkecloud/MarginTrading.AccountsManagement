using MarginTrading.AccountsManagement.Contracts.Events;

namespace MarginTrading.AccountsManagement.TradingEngineMock.Contracts
{
    public class AmountForWithdrawalFrozenEvent : AccountBalanceOperationEventBase
    {
        public AmountForWithdrawalFrozenEvent(string clientId, string accountId, decimal amount, string operationId,
            string reason) 
            : base(clientId, accountId, amount, operationId, reason)
        {
        }
    }
}