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
            string failReason, string eventSourceId)
            : base(operationId, eventTimestamp)
        {
            FailReason = failReason;
            EventSourceId = eventSourceId;
        }
     
        [Key(2)] 
        public string FailReason { get; }

        [Key(3)]
        public string EventSourceId { get; set; }
    }
}