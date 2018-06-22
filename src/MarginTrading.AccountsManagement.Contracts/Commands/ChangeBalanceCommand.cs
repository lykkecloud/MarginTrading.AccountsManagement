﻿using System;
using MarginTrading.AccountsManagement.Contracts.Models;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Give ability to change account balance when taking commissions, fees
    /// and all making any other balance change, except deposit, withdraw and realized PnL.
    /// </summary>
    [MessagePackObject]
    public class ChangeBalanceCommand
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// It would be a nice idea to use a <see cref="Guid"/> here.<br/>
        /// </summary>
        /// <remarks>
        /// Not required. If not provided it is autogenerated.
        /// </remarks>
        [Key(0)]
        public string OperationId { get; }

        /// <summary>
        /// Client Id.
        /// </summary>
        [Key(1)]
        public string ClientId { get; }

        /// <summary>
        /// Account Id. Must be a client's account.
        /// </summary>
        [Key(2)]
        public string AccountId { get; }

        /// <summary>
        /// The amount of money to add or deduct from the account's balance 
        /// </summary>
        [Key(3)]
        public decimal Amount { get; }

        /// <summary>
        /// Reason of balance changing.
        /// </summary>
        [Key(4)]
        public AccountBalanceChangeReasonTypeContract ReasonType { get; }
        
        /// <summary>
        /// Reason of modification.
        /// </summary>
        [Key(5)]
        public string Reason { get; set; }

        /// <summary>
        /// Any additional information.
        /// </summary>
        [Key(6)]
        public string AuditLog { get; }
    }
}