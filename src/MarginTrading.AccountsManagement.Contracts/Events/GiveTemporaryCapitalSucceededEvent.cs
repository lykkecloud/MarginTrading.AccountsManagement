using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Give Temporary Capital operation succeeded
    /// </summary>
    [MessagePackObject]
    public class GiveTemporaryCapitalSucceededEvent : BaseEvent
    {
        public GiveTemporaryCapitalSucceededEvent([NotNull] string operationId, DateTime eventTimestamp, 
            string eventSourceId, string accountId, decimal amount, string reason, string auditLog)
            : base(operationId, eventTimestamp)
        {
            EventSourceId = eventSourceId;
            AccountId = accountId;
            Amount = amount;
            Reason = reason;
            AuditLog = auditLog;
        }
        
        [Key(2)]
        public string EventSourceId { get; }
        
        [Key(3)]
        public string AccountId { get; }
        
        [Key(4)]
        public decimal Amount { get; }
        
        [Key(5)]
        public string Reason { get; }
        
        [Key(6)]
        public string AuditLog { get; }
    }
}