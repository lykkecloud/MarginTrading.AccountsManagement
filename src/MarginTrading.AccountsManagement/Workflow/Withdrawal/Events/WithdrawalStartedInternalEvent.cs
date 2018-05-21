using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal.Events
{
    /// <summary>
    /// Withdrawal started
    /// </summary>
    [MessagePackObject]
    public class WithdrawalStartedInternalEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        public WithdrawalStartedInternalEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}