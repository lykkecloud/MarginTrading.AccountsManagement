using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Withdrawal operation failed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalFailedEvent : BaseEvent
    {
        [Key(2)] 
        public string Reason { get; }
        
        [Key(3)]
        public string AccountId { get; }
        
        [Key(4)]
        public string ClientId { get; }
        
        [Key(5)]
        public decimal Amount { get; }

        public WithdrawalFailedEvent([NotNull] string operationId, DateTime eventTimestamp, string reason,
            string accountId, string clientId, decimal amount)
            : base(operationId, eventTimestamp)
        {
            Reason = reason;
            AccountId = accountId;
            ClientId = clientId;
            Amount = amount;
        }
    }
}