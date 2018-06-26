using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Margin unfreezing has succeeded
    /// </summary>
    [MessagePackObject]
    public class UnfreezeMarginSucceededWithdrawalEvent : BaseEvent
    {
        [Key(2)]
        public string ClientId { get; }

        [Key(3)]
        public string AccountId { get; }

        [Key(4)]
        public decimal Amount { get; }

        public UnfreezeMarginSucceededWithdrawalEvent([NotNull] string operationId, DateTime _, 
            [NotNull] string clientId, [NotNull] string accountId, decimal amount)
            : base(operationId)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}