using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    [MessagePackObject]
    public class WithdrawalStartFailedInternalEvent : BaseEvent
    {
        [Key(2)]
        public string ClientId { get; }

        [Key(3)]
        public string AccountId { get; }

        [Key(4)]
        public decimal Amount { get; }
        
        [Key(5)]
        public string Reason { get; }

        public WithdrawalStartFailedInternalEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string clientId, [NotNull] string accountId, decimal amount, [NotNull] string reason)
            : base(operationId, eventTimestamp)
        {
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}