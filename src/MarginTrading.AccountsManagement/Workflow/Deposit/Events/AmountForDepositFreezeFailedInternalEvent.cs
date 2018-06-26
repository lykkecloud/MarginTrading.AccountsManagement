using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class AmountForDepositFreezeFailedInternalEvent: BaseEvent
    {
        [Key(2)]
        public string Reason { get; }

        public AmountForDepositFreezeFailedInternalEvent([NotNull] string operationId, 
            [NotNull] string reason)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}