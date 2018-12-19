using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Commands
{
    [MessagePackObject]
    public class StartGiveTemporaryCapitalInternalCommand
    {
        public StartGiveTemporaryCapitalInternalCommand([NotNull] string operationId,
            string eventSourceId, string accountId, decimal amount, string reason, string auditLog)
        {
            OperationId = operationId;
            EventSourceId = eventSourceId;
            AccountId = accountId;
            Amount = amount;
            Reason = reason;
            AuditLog = auditLog;
        }
        
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public string EventSourceId { get; }
        
        [Key(2)]
        public string AccountId { get; }
        
        [Key(3)]
        public decimal Amount { get; }
        
        [Key(4)]
        public string Reason { get; }
        
        [Key(5)]
        public string AuditLog { get; }
    }
}