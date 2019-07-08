// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Commands
{
    [MessagePackObject]
    public class StartGiveTemporaryCapitalInternalCommand
    {
        public StartGiveTemporaryCapitalInternalCommand([NotNull] string operationId,
            string accountId, decimal amount, string reason, string comment, string additionalInfo)
        {
            OperationId = operationId;
            AccountId = accountId;
            Amount = amount;
            Reason = reason;
            Comment = comment;
            AdditionalInfo = additionalInfo;
        }
        
        [Key(0)]
        public string OperationId { get; }
        
        [Key(1)]
        public string AccountId { get; }
        
        [Key(2)]
        public decimal Amount { get; }
        
        [Key(3)]
        public string Reason { get; }
        
        [Key(4)]
        public string Comment { get; }
        
        [Key(5)]
        public string AdditionalInfo { get; }
    }
}