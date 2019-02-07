using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands
{
    [MessagePackObject]
    public class FinishAccountsDeletionInternalCommand
    {
        /// <summary>
        /// The unique id of operation.<br/>
        /// Two operations with equal type and id are considered one operation, all duplicates are skipped.<br/>
        /// </summary>
        [CanBeNull]
        [Key(0)]
        public string OperationId { get; set; }
    }
}