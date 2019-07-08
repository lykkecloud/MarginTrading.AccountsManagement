// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Base event containing operation Id and timestamp
    /// </summary>
    [MessagePackObject]
    public abstract class BaseEvent
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// </summary>
        [Key(0)]
        public string OperationId { get; }
        
        /// <summary>
        /// Time of event creation.
        /// </summary>
        [Key(1)]
        public DateTime EventTimestamp { get; }

        protected BaseEvent([NotNull] string operationId, DateTime eventTimestamp)
        {
            OperationId = operationId;
            EventTimestamp = eventTimestamp;
        }
    }
}