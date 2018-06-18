using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Withdrawal operation failed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalFailedEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public string Reason { get; }
        
        public WithdrawalFailedEvent([NotNull] string operationId, [NotNull] string reason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}