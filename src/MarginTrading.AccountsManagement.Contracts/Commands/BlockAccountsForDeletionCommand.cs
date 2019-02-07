using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Command to block accounts for deletion on side of Trading Engine.
    /// </summary>
    [MessagePackObject]
    public class BlockAccountsForDeletionCommand
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; set; }

        /// <summary>
        /// List of account id's to be blocked.
        /// </summary>
        [NotNull]
        [Key(1)]
        public List<string> AccountIds { get; set; }
    }
}