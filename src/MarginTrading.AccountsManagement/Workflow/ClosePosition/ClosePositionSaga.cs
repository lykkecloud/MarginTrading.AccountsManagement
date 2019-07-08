// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.Backend.Contracts.Events;

namespace MarginTrading.AccountsManagement.Workflow.ClosePosition
{
    internal class ClosePositionSaga
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly CqrsContextNamesSettings _contextNames;

        public ClosePositionSaga(
            IChaosKitty chaosKitty,
            CqrsContextNamesSettings contextNames)
        {
            _chaosKitty = chaosKitty;
            _contextNames = contextNames;
        }

        /// <summary>
        /// The position is closed => update the balance
        /// </summary>
        [UsedImplicitly]
        private void Handle(PositionClosedEvent evt, ICommandSender sender)
        {
            var operationId = evt.PositionId + "-update-balance";
            
            sender.SendCommand(
                new UpdateBalanceInternalCommand(
                    operationId: operationId,
                    accountId: evt.AccountId,
                    amountDelta: evt.BalanceDelta,
                    comment: $"Balance changed on position close (id = {evt.PositionId})",
                    auditLog: string.Empty,
                    source: nameof(ClosePositionSaga),
                    changeReasonType: AccountBalanceChangeReasonType.RealizedPnL,
                    eventSourceId: evt.PositionId,
                    assetPairId: evt.AssetPairId,
                    tradingDay: DateTime.UtcNow),
                _contextNames.AccountsManagement);
        }
    }
}