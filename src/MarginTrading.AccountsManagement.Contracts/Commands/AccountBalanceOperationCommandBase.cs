using System;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    [MessagePackObject]
    public abstract class AccountBalanceOperationCommandBase
    {
        [Key(0)] public string ClientId { get; }
        [Key(1)] public string AccountId { get; }
        [Key(2)] public decimal Amount { get; }
        [Key(3)] public string OperationId { get; }
        [Key(4)] public string Reason { get; }

        protected AccountBalanceOperationCommandBase(string clientId, string accountId, decimal amount,
            string operationId, string reason)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason  ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}