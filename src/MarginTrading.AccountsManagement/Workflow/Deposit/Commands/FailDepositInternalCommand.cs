using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Commands
{
    [MessagePackObject]
    internal class FailDepositInternalCommand
    {
        [Key(0)]
        public string OperationId { get; }

        [Key(1)]
        public string Reason { get; }

        public FailDepositInternalCommand([NotNull] string operationId, [NotNull] string reason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}