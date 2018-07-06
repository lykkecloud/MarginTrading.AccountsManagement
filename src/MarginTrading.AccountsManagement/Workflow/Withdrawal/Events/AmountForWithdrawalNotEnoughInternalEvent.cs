using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    [MessagePackObject]
    public class AmountForWithdrawalNotEnoughInternalEvent : BaseEvent
    {
        [Key(2)]
        public string ClientId { get; }

        [Key(3)]
        public string AccountId { get; }

        [Key(4)]
        public decimal Amount { get; }

        public AmountForWithdrawalNotEnoughInternalEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string clientId, [NotNull] string accountId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
        }
    }
}