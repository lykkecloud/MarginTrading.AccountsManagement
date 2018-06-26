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

        public AmountForDepositFreezeFailedInternalEvent([NotNull] string operationId, DateTime _, 
            [NotNull] string reason)
            : base(operationId)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}