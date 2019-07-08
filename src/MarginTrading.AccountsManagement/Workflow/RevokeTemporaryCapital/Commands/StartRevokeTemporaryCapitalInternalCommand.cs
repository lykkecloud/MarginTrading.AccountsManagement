// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands
{
    [MessagePackObject]
    public class StartRevokeTemporaryCapitalInternalCommand
    {
        public StartRevokeTemporaryCapitalInternalCommand([NotNull] string operationId,
            string accountId, string revokeEventSourceId, string comment, string additionalInfo)
        {
            OperationId = operationId;
            AccountId = accountId;
            RevokeEventSourceId = revokeEventSourceId;
            Comment = comment;
            AdditionalInfo = additionalInfo;
        }
        
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public string AccountId { get; }
        
        [Key(2)]
        public string RevokeEventSourceId { get; }
        
        [Key(3)]
        public string Comment { get; }
        
        [Key(4)]
        public string AdditionalInfo { get; }
    }
}