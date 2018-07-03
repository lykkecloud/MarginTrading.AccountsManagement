using System;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.AccountsManagement.Workflow.ClosePosition
{
    internal class ClosePositionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;

        public ClosePositionSaga(CqrsContextNamesSettings contextNames)
        {
            _contextNames = contextNames;
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
                    changeReasonType: AccountBalanceChangeReasonType.RealizedPnL,
                    eventSourceId: evt.PositionId,
                    assetPairId: string.Empty, //TODO pass through ClosePositionSaga from MT Core https://lykke-snow.atlassian.net/browse/MTC-200
                    tradingDay: DateTime.UtcNow),
                _contextNames.AccountsManagement);
        }
    }
}