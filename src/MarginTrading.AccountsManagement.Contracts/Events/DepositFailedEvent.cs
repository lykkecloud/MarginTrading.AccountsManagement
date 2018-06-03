using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Deposit operation failed
    /// </summary>
    [MessagePackObject]
    public class DepositFailedEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        public DepositFailedEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}