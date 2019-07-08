// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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