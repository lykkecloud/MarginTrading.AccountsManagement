using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class AmountForDepositFrozenInternalEvent
    {
        [Key(0)]
        public string OperationId { get; }

        public AmountForDepositFrozenInternalEvent([NotNull] string operationId)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        }
    }
}