using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Events;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    [MessagePackObject]
    public class WithdrawalStartFailedInternalEvent : BaseEvent
    {
  

        public WithdrawalStartFailedInternalEvent([NotNull] string operationId, DateTime eventTimestamp)
            : base(operationId, eventTimestamp)
        {
          
        }
    }
}