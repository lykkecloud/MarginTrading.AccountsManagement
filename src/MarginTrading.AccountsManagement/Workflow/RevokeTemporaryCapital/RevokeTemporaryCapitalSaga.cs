// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Events;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital
{
    internal class RevokeTemporaryCapitalSaga
    {
        internal const string OperationName = "RevokeTemporaryCapital";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public RevokeTemporaryCapitalSaga(
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
        /// Temporary capital is removed from account, start account balance update
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public async Task Handle(RevokeTemporaryCapitalStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Initiated, TemporaryCapitalState.Started))
            {
                executionInfo.Data.RevokedTemporaryCapital = e.RevokedTemporaryCapital;
                
                sender.SendCommand(
                    new UpdateBalanceInternalCommand(
                        e.OperationId,
                        executionInfo.Data.AccountId,
                        - executionInfo.Data.RevokedTemporaryCapital.Sum(x => x.Amount),
                        $"{executionInfo.Data.Comment}  ***  Revoked eventSourceIds: [{string.Join(",", executionInfo.Data.RevokedTemporaryCapital.Select(x => x.Id))}]",
                        executionInfo.Data.AdditionalInfo,
                        OperationName,
                        AccountBalanceChangeReasonType.TemporaryCashAdjustment,
                        executionInfo.Data.EventSourceId,
                        string.Empty,
                        _systemClock.UtcNow.UtcDateTime),
                    _contextNames.AccountsManagement);   

                _chaosKitty.Meow(
                    $"{nameof(RevokeTemporaryCapitalStartedInternalEvent)}: " +
                    "Save_OperationExecutionInfo:" +
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

            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(OperationName, e.BalanceChange.Id);

            if (executionInfo == null)
                return;

            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Started, TemporaryCapitalState.ChargedOnAccount))
            {
                sender.SendCommand(
                    new FinishRevokeTemporaryCapitalInternalCommand(
                        e.BalanceChange.Id,
                        _systemClock.UtcNow.UtcDateTime,
                        true,
                        null),
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
            
            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo == null)
                return;
            
            if (executionInfo.Data.SwitchState(TemporaryCapitalState.Started, TemporaryCapitalState.Failing))
            {
                executionInfo.Data.FailReason = e.Reason;
                sender.SendCommand(
                    new FinishRevokeTemporaryCapitalInternalCommand(
                        e.OperationId,
                        _systemClock.UtcNow.UtcDateTime,
                        false,
                        e.Reason),
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
        private async Task Handle(RevokeTemporaryCapitalSucceededEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(OperationName, e.OperationId);

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
        private async Task Handle(RevokeTemporaryCapitalFailedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo != null
                && new[] {TemporaryCapitalState.Initiated, TemporaryCapitalState.Failing}.Contains(executionInfo.Data.State)
                && executionInfo.Data.SwitchState(executionInfo.Data.State,
                    TemporaryCapitalState.Failed))
            {
                executionInfo.Data.FailReason = e.FailReason;

                await _executionInfoRepository.SaveAsync(executionInfo);
            }
        }
    }
}