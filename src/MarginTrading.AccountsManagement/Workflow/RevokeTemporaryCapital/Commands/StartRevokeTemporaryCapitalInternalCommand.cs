using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands
{
    [MessagePackObject]
    public class StartRevokeTemporaryCapitalInternalCommand
    {
        public StartRevokeTemporaryCapitalInternalCommand([NotNull] string operationId,
            string eventSourceId, string accountId, string revokeEventSourceId, string auditLog)
        {
            OperationId = operationId;
            EventSourceId = eventSourceId;
            AccountId = accountId;
            RevokeEventSourceId = revokeEventSourceId;
            AuditLog = auditLog;
        }
        
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public string EventSourceId { get; }
        
        [Key(2)]
        public string AccountId { get; }
        
        [Key(3)]
        public string RevokeEventSourceId { get; }
        
        [Key(4)]
        public string AuditLog { get; }
    }
}