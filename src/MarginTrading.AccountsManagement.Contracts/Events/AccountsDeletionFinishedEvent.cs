using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Event is generated when accounts deletion process is finished
    /// </summary>
    [MessagePackObject]
    public class AccountsDeletionFinishedEvent: BaseEvent
    {
        /// <summary>
        /// List of successfully deleted accounts.
        /// </summary>
        [Key(2)]
        public List<string> DeletedAccountIds { get; }
        
        /// <summary>
        /// Dictionary of failed accounts with reason.
        /// </summary>
        [Key(3)]
        public Dictionary<string, string> FailedAccountIds { get; }
        
        /// <summary>
        /// Comment that was passed at operation start.
        /// </summary>
        [Key(4)]
        public string Comment { get; }
        
        public AccountsDeletionFinishedEvent([NotNull] string operationId, DateTime eventTimestamp, 
            List<string> deletedAccountIds, Dictionary<string, string> failedAccountIds, string comment) 
            : base(operationId, eventTimestamp)
        {
            DeletedAccountIds = deletedAccountIds;
            FailedAccountIds = failedAccountIds;
            Comment = comment;
        }
    }
}