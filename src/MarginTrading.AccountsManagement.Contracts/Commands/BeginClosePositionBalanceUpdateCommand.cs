using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Commands
{
    /// <summary>
    /// Position closed, update the balance
    /// </summary>
    public class BeginClosePositionBalanceUpdateCommand : AccountBalanceOperationCommandBase
    {
        /// <summary>
        /// The source position id
        /// </summary>
        [Key(5)]
        public string EventSourceId { get; }

        public BeginClosePositionBalanceUpdateCommand(string clientId, string accountId, decimal amount,
            string operationId, string reason, string eventSourceId) : base(clientId, accountId, amount, operationId,
            reason)
        {
            EventSourceId = eventSourceId;
        }
    }
}