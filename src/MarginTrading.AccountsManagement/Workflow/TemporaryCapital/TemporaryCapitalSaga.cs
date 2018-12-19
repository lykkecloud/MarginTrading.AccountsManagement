using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Events;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.TemporaryCapital
{
    internal class TemporaryCapitalSaga
    {
        internal const string OperationName = "TemporaryCapital";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public TemporaryCapitalSaga(
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
        
        #region GiveTemporaryCapital

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

            if (executionInfo.Data.SwitchState(GiveTemporaryCapitalState.Initiated, GiveTemporaryCapitalState.Started))
            {
                sender.SendCommand(
                    new UpdateBalanceInternalCommand(
                        operationId: e.OperationId,
                        accountId: executionInfo.Data.AccountId,
                        amountDelta: executionInfo.Data.Amount,
                        comment: "Give temporary capital",
                        auditLog: executionInfo.Data.AuditLog,
                        source: OperationName,
                        changeReasonType: AccountBalanceChangeReasonType.TemporaryCashAdjustment,
                        eventSourceId: executionInfo.Data.EventSourceId,
                        assetPairId: string.Empty,
                        tradingDay: _systemClock.UtcNow.UtcDateTime),
                    _contextNames.AccountsManagement);   

                _chaosKitty.Meow(
                    $"{nameof(GiveTemporaryCapitalStartedInternalEvent)}: " +
                    "Save_OperationExecutionInfo:" +
                    $"{e.OperationId}");
                
                await _executionInfoRepository.Save(executionInfo);
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

            if (executionInfo.Data.SwitchState(GiveTemporaryCapitalState.Started, GiveTemporaryCapitalState.ChargedOnAccount))
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
                
                await _executionInfoRepository.Save(executionInfo);
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
            
            if (executionInfo.Data.SwitchState(GiveTemporaryCapitalState.Started, GiveTemporaryCapitalState.Failing))
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
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Operation succeeded
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(GiveTemporaryCapitalSucceededEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo != null && executionInfo.Data.SwitchState(GiveTemporaryCapitalState.ChargedOnAccount, 
                    GiveTemporaryCapitalState.Succeded))
            {
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Operation failed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(GiveTemporaryCapitalFailedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, e.OperationId);

            if (executionInfo != null && executionInfo.Data.SwitchState(GiveTemporaryCapitalState.Initiated, 
                    GiveTemporaryCapitalState.Failed))
            {
                executionInfo.Data.FailReason = e.FailReason;
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }
        
        #endregion GiveTemporaryCapital
        
        #region RevokeTemporaryCapital
        
        
        
        #endregion RevokeTemporaryCapital
    }
}