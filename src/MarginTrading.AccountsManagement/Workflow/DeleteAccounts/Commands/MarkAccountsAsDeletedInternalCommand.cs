using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands
{
    [MessagePackObject]
    public class MarkAccountsAsDeletedInternalCommand
    {
        [Key(0)]
        public string OperationId { get; set; }
    }
}