using System;

namespace MarginTrading.AccountsManagement.Workflow.Commands
{
    internal class UpdateBalanceInternalCommand
    {
        public string ClientId { get; }
        public string AccountId { get; }
        public decimal AmountDelta { get; }
        public string OperationId { get; }
        public string Reason { get; }

        public UpdateBalanceInternalCommand(string userId, string accountId, decimal amountDelta, string operationId,
            string reason)
        {
            ClientId = userId ?? throw new ArgumentNullException(nameof(userId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            AmountDelta = amountDelta;
            OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}