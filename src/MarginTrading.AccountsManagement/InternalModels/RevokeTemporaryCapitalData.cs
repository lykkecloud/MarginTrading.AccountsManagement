using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class RevokeTemporaryCapitalData : OperationDataBase<TemporaryCapitalState>
    {
        public string OperationId { get; set; }

        public string EventSourceId { get; set; }

        public string AccountId { get; set; }
        
        public string RevokeEventSourceId { get; set; }

        public string AuditLog { get; set; }
        
        public string FailReason { get; set; }
        
        public List<TemporaryCapital> RevokedTemporaryCapital { get; set; }
    }
}