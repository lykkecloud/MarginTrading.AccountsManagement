using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    [MessagePackObject]
    public class AccountBalanceChangeFailedEvent : BaseEvent
    {
        [Key(2)]
        public string Reason { get; }

        public AccountBalanceChangeFailedEvent([NotNull] string operationId, DateTime _, 
            [NotNull] string reason)
            : base(operationId)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}