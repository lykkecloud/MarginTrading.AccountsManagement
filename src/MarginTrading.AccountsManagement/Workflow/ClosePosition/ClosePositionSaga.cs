using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.AccountsManagement.Workflow.ClosePosition
{
    internal class ClosePositionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IOperationStatesRepository _operationStatesRepository;

        public ClosePositionSaga(CqrsContextNamesSettings contextNames, IOperationStatesRepository operationStatesRepository)
        {
            _contextNames = contextNames;
            _operationStatesRepository = operationStatesRepository;
        }

        /// <summary>
        /// The position is closed => udpate the balance
        /// </summary>
        [UsedImplicitly]
        private void Handle(PositionClosedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(
                new UpdateBalanceInternalCommand(
                    operationId: evt.PositionId + "-update-balance",
                    clientId: evt.ClientId,
                    accountId: evt.AccountId,
                    amountDelta: evt.BalanceDelta,
                    comment: $"Balance changed on position close (id = {evt.PositionId})",
                    auditLog: string.Empty,
                    source: nameof(ClosePositionSaga),
                    changeReasonType: AccountBalanceChangeReasonType.PositionClosed),
                _contextNames.AccountsManagement);
        }
        
        
    }
}