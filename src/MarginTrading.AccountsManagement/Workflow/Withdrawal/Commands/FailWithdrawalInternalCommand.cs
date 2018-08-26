using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands
{
    [MessagePackObject]
    internal class FailWithdrawalInternalCommand
    {
        [Key(0)]
        public string OperationId { get; }
       

        public FailWithdrawalInternalCommand([NotNull] string operationId)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        }
    }
}