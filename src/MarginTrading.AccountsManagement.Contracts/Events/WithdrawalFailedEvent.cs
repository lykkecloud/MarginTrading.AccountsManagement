using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Withdrawal operation failed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalFailedEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        public WithdrawalFailedEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}