// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Event is generated when accounts are removed from cache
    /// and trades & withdrawals are unblocked for failed accounts.
    /// </summary>
    public class MtCoreDeleteAccountsFinishedEvent: BaseEvent
    {
        public MtCoreDeleteAccountsFinishedEvent([NotNull] string operationId, DateTime eventTimestamp) 
            : base(operationId, eventTimestamp)
        {
        }
    }
}