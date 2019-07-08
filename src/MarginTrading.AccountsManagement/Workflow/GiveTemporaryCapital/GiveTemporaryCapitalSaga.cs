// Copyright (c) 2019 Lykke Corp.

using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Events;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital
{
    internal class GiveTemporaryCapitalSaga
    {
        internal const string OperationName = "GiveTemporaryCapital";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public GiveTemporaryCapitalSaga(
            CqrsContextNamesSettings contextNames,
            ISystemClock systemClock,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
        }
        
        /// <summary>
        /// Temporary capital is saved on account, start account balance update
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task Handle(GiveTemporaryCapitalStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Initiated, TemporaryCapitalState.Started))
            {
                sender.SendCommand(
                    new UpdateBalanceInternalCommand(
                        operationId: e.OperationId,
                        accountId: executionInfo.Data.AccountId,
                        amountDelta: executionInfo.Data.Amount,
                        comment: executionInfo.Data.Comment,
                        auditLog: executionInfo.Data.AdditionalInfo,
                        source: OperationName,
                        changeReasonType: AccountBalanceChangeReasonType.TemporaryCashAdjustment,
                        eventSourceId: e.OperationId,
                        assetPairId: string.Empty,
                        tradingDay: _systemClock.UtcNow.UtcDateTime),
                    _contextNames.AccountsManagement);

                _chaosKitty.Meow(
                    $"{nameof(GiveTemporaryCapitalStartedInternalEvent)}: " +
                    "Save_OperationExecutionInfo: " +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
        
        /// <summary>
        /// The balance has changed => finish the operation
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountChangedEvent e, ICommandSender sender)
        {
            if (e.Source != OperationName)
                return;

            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.BalanceChange.Id);

            if (executionInfo == null)
                return;

            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Started, TemporaryCapitalState.ChargedOnAccount))
            {
                sender.SendCommand(
                    new FinishGiveTemporaryCapitalInternalCommand(
                        operationId: e.BalanceChange.Id,
                        eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                        isSuccess: true,
                        failReason: null),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow($"{nameof(AccountChangedEvent)}: " +
                                 "Save_OperationExecutionInfo:" + e.BalanceChange.Id);
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }

        /// <summary>
        /// Failed to change account balance => fail the operation
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountBalanceChangeFailedEvent e, ICommandSender sender)
        {
            if (e.Source != OperationName)
                return;
            
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo == null)
                return;
            
            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Started, TemporaryCapitalState.Failing))
            {
                executionInfo.Data.FailReason = e.Reason;
                sender.SendCommand(
                    new FinishGiveTemporaryCapitalInternalCommand(
                        operationId: e.OperationId,
                        eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                        isSuccess: false,
                        failReason: e.Reason),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow($"{nameof(AccountBalanceChangeFailedEvent)}: " +
                                 "Save_OperationExecutionInfo:" + e.OperationId);
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }

        /// <summary>
        /// Operation succeeded
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(GiveTemporaryCapitalSucceededEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo != null && executionInfo.Data.SwitchState(TemporaryCapitalState.ChargedOnAccount, 
                    TemporaryCapitalState.Succeded))
            {
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }

        /// <summary>
        /// Operation failed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(GiveTemporaryCapitalFailedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo != null 
                && new [] {TemporaryCapitalState.Initiated, TemporaryCapitalState.Failing}.Contains(executionInfo.Data.State)
                && executionInfo.Data.SwitchState(executionInfo.Data.State, 
                    TemporaryCapitalState.Failed))
            {
                executionInfo.Data.FailReason = e.FailReason;
                
                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
    }
}