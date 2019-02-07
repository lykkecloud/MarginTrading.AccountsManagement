using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Command to launch accounts deletion process.
    /// </summary>
    [MessagePackObject]
    public class DeleteAccountsCommand
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
        [Key(0)]
        public string OperationId { get; set; }

        /// <summary>
        /// List of account id's to be deleted.
        /// </summary>
        [NotNull]
        [Key(1)]
        public List<string> AccountIds { get; set; }
        
        /// <summary>
        /// Any comment to the operation.
        /// </summary>
        [CanBeNull]
        [Key(2)]
        public string Comment { get; set; }
    }
}