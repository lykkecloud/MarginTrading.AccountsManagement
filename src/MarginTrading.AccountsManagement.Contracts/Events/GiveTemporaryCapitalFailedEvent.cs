using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Give Temporary Capital operation failed
    /// </summary>
    [MessagePackObject]
    public class GiveTemporaryCapitalFailedEvent : BaseEvent
    {
        public GiveTemporaryCapitalFailedEvent([NotNull] string operationId, DateTime eventTimestamp, 
            string failReason)
            : base(operationId, eventTimestamp)
        {
            FailReason = failReason;
        }
     
        [Key(2)] 
        public string FailReason { get; }
    }
}