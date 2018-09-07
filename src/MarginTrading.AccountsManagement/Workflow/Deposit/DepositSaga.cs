using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.Deposit.Events;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    internal class DepositSaga
    {
        private const string OperationName = "Deposit";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public DepositSaga(CqrsContextNamesSettings contextNames,
            IOperationExecutionInfoRepository executionInfoRepository, IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// The deposit has started => freeze the amount to be deposited.
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DepositStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
                return; 

            if (SwitchState(executionInfo.Data, State.Created, State.FreezingAmount))
            {
                sender.SendCommand(
                    new FreezeAmountForDepositInternalCommand(e.OperationId),
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
                
        }

        /// <summary>
        /// The amount was frozen the in the margin => update the balance
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AmountForDepositFrozenInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(OperationName, e.OperationId);

            if (executionInfo == null)
                return;

            if (SwitchState(executionInfo.Data, State.FreezingAmount, State.UpdatingBalance))
            {
                sender.SendCommand(
                    new UpdateBalanceInternalCommand(
                        operationId: e.OperationId,
                        clientId: executionInfo.Data.ClientId,
                        accountId: executionInfo.Data.AccountId,
                        amountDelta: executionInfo.Data.Amount,
                        comment: "Funds deposit " + executionInfo.Data.Comment,
                        auditLog: executionInfo.Data.AuditLog,
                        source: OperationName,
                        changeReasonType: AccountBalanceChangeReasonType.Deposit,
                        eventSourceId: e.OperationId,
                        assetPairId: string.Empty,
                        tradingDay: DateTime.UtcNow),
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Failed to freeze the amount in the margin => fail the deposit
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AmountForDepositFreezeFailedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(OperationName, e.OperationId);

            if (executionInfo == null)
                return;

            if (SwitchState(executionInfo.Data, State.FreezingAmount, State.Failed))
            {
         
                sender.SendCommand(
                    new FailDepositInternalCommand(e.OperationId),
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
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

            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(OperationName, e.BalanceChange.Id);

            if (executionInfo == null)
                return;

            if (SwitchState(executionInfo.Data, State.UpdatingBalance, State.Succeeded))
            {
                sender.SendCommand(
                    new CompleteDepositInternalCommand(
                        operationId: e.BalanceChange.Id),
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.BalanceChange.Id);
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Failed to change account balance => fail the deposit
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountBalanceChangeFailedEvent e, ICommandSender sender)
        {
            if (e.Source != OperationName)
                return;
            
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(OperationName, e.OperationId);

            if (executionInfo == null)
                return;
            
            if (SwitchState(executionInfo.Data, State.UpdatingBalance, State.Failed))
            {
                executionInfo.Data.FailReason = e.Reason;
                sender.SendCommand(
                    new FailDepositInternalCommand(e.OperationId),
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
        }
        
        
        private static bool SwitchState(WithdrawalDepositData data, State expectedState, State nextState)
        {
            if (data.State < expectedState)
            {
                // Throws to retry and wait until the operation will be in the required state
                throw new InvalidOperationException(
                    $"Operation execution state can't be switched: {data.State} -> {nextState}. Waiting for the {expectedState} state.");
            }

            if (data.State > expectedState)
            {
                // Already in the next state, so this event can be just ignored
                return false;
            }

            data.State = nextState;

            return true;
        }

    }
}