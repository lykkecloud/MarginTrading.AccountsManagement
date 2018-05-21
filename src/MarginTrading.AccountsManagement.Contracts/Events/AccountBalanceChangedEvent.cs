using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Account balance changed
    /// </summary>
    public class AccountBalanceChangedEvent
    {
        /// <inheritdoc />
        public AccountBalanceChangedEvent(string operationId, string source, AccountBalanceChangeContract change,
            AccountContract account)
        {
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Change = change ?? throw new ArgumentNullException(nameof(change));
            Account = account ?? throw new ArgumentNullException(nameof(account));
        }

        /// <summary>
        /// Id of operation
        /// </summary>
        [Key(0)]
        public string OperationId { get; }

        /// <summary>
        /// Source of change
        /// </summary>
        [Key(1)]
        public string Source { get; }

        /// <summary>
        /// Change details
        /// </summary>
        [Key(2)]
        public AccountBalanceChangeContract Change { get; }

        /// <summary>
        /// Resulting contract
        /// </summary>
        [Key(3)]
        public AccountContract Account { get; }
    }
}