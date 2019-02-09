using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Command to remove accounts from cache and unblock trades and withdrawals for failed accounts.
    /// </summary>
    [MessagePackObject]
    public class MtCoreFinishAccountsDeletionCommand
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// </summary>
        [NotNull]
        [Key(0)]
        public string OperationId { get; set; }

        /// <summary>
        /// List of account id's to be deleted.
        /// </summary>
        [NotNull]
        [Key(1)]
        public List<string> AccountIds { get; set; }

        /// <summary>
        /// List of account id's to be unblocked.
        /// </summary>
        [NotNull]
        [Key(1)]
        public List<string> FailedAccountIds { get; set; }
    }
}