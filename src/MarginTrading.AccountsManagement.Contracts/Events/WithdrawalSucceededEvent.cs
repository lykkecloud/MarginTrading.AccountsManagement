using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Happens when the withdrawal is completed
    /// </summary>
    [MessagePackObject]
    public class WithdrawalSucceededEvent
    {
        [Key(0)]
        public string OperationId { get; }
        
        public WithdrawalSucceededEvent(string operationId)
        {
            OperationId = operationId;
        }
    }
}