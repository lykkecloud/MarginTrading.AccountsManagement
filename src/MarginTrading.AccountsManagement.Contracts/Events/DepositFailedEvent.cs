using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Deposit operation failed
    /// </summary>
    [MessagePackObject]
    public class DepositFailedEvent : BaseEvent
    {   
        public DepositFailedEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}