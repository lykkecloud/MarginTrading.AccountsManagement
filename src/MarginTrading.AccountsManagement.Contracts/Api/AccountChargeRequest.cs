﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class AccountChargeRequest
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// It would be a nice idea to use a <see cref="Guid"/> here.<br/>
        /// </summary>
        /// <remarks>
        /// Not required. If not provided it is autogenerated.
        /// </remarks>
        [CanBeNull]
        public string OperationId { get; set; }

        /// <summary>
        /// The amount of money to add or deduct from the account's balance 
        /// </summary>
        public decimal AmountDelta { get; set; }

        /// <summary>
        /// Reason of modification
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Additional information may be passed from call initiator.
        /// Info will be saved to history in AuditLog field.
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Asset Pair ID (if can be found any)
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// Operation trading date
        /// </summary>
        public DateTime? TradingDay { get; set; }
    }
}