using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Deposit.Events
{
    [MessagePackObject]
    internal class AmountForDepositFrozenInternalEvent: BaseEvent
    {
        public AmountForDepositFrozenInternalEvent([NotNull] string operationId, DateTime _ = default(DateTime))
            : base(operationId)
        {
        }
    }
}