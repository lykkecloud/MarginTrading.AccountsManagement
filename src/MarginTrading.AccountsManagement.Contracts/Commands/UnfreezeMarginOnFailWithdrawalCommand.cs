using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    [MessagePackObject]
    public class UnfreezeMarginOnFailWithdrawalCommand
    {
        [Key(0)][NotNull]
        public string OperationId { get; }

        [Key(1)][NotNull]
        public string ClientId { get; }

        [Key(2)][NotNull]
        public string AccountId { get; }

        [Key(3)]
        public decimal Amount { get; }

        public UnfreezeMarginOnFailWithdrawalCommand([NotNull] string operationId, [NotNull] string clientId,
            [NotNull] string accountId, decimal amount)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}