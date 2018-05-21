using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// The deposit operation has succeeded
    /// </summary>
    [MessagePackObject]
    public class DepositSucceededEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        public DepositSucceededEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}