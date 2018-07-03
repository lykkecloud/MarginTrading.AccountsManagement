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
        
        [Key(3)]
        public string Source { get; }

        public AccountBalanceChangeFailedEvent([NotNull] string operationId, DateTime eventTimestamp, 
            [NotNull] string reason, string source)
            : base(operationId, eventTimestamp)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Source = source;
        }
    }
}