using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalSaga
    {
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly CqrsContextNamesSettings _contextNames;
        private const string OperationName = "Withdraw";
        private readonly IChaosKitty _chaosKitty;

        public WithdrawalSaga(CqrsContextNamesSettings contextNames,
            IOperationExecutionInfoRepository executionInfoRepository, IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// The withdrawal has started => ask the backend to freeze the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(WithdrawalStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: e.OperationId,
                factory: () => new OperationExecutionInfo<DepositData>(
                    operationName: OperationName,
                    id: e.OperationId,
                    data: new DepositData
                    {
                        ClientId = e.ClientId,
                        AccountId = e.AccountId,
                        Amount = e.Amount,
                        AuditLog = e.AuditLog,
                        State = State.FreezingAmount,
                        Comment = e.Comment, 
                    }));

            _chaosKitty.Meow(e.OperationId);

            if (executionInfo.Data.State == State.FreezingAmount)
                sender.SendCommand(
                    new FreezeAmountForWithdrawalCommand(
                        clientId: e.OperationId,
                        accountId: e.AccountId,
                        amount: e.Amount,
                        operationId: e.OperationId,
                        reason: string.Empty),
                    _contextNames.TradingEngine);
        }

        /// <summary>
        /// The backend frozen the amount in the margin => update the balance
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AmountForWithdrawalFrozenEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.FreezingAmount, State.UpdatingBalance))
            {
                sender.SendCommand(
                    new UpdateBalanceInternalCommand(
                        operationId: e.OperationId,
                        clientId: executionInfo.Data.ClientId,
                        accountId: executionInfo.Data.AccountId,
                        amountDelta: -executionInfo.Data.Amount,
                        comment: "Funds withdrawal " + e.OperationId,
                        auditLog: executionInfo.Data.AuditLog,
                        source: OperationName,
                        changeReasonType: AccountBalanceChangeReasonType.Withdraw),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(e.OperationId);

                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// The backend failed to freeze the amount in the margin => fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AmountForWithdrawalFreezeFailedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.FreezingAmount, State.Failed))
            {
                executionInfo.Data.FailReason = e.Reason;
                sender.SendCommand(
                    new FailWithdrawalInternalCommand(e.OperationId, "Failed to freeze amount for deposit: " + e.Reason), 
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// The balance has changed => notify TradingCode that withdrawal has failed to unfreeze the margin
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountBalanceChangedEvent e, ICommandSender sender)
        {
            if (e.Source != OperationName)
                return;

            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.UpdatingBalance, State.UnfreezingAmount))
            {
                sender.SendCommand(
                    new UnfreezeMarginWithdrawalCommand(
                        operationId: e.OperationId,
                        clientId: executionInfo.Data.ClientId,
                        accountId: executionInfo.Data.AccountId,
                        amount: executionInfo.Data.Amount),
                    _contextNames.TradingEngine);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Notify TradingCode that withdrawal has failed to unfreeze the margin
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountBalanceChangeFailedEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.UpdatingBalance, State.UnfreezingAmount))
            {
                executionInfo.Data.FailReason = e.Reason;
                sender.SendCommand(
                    new UnfreezeMarginOnFailWithdrawalCommand(
                        operationId: e.OperationId,
                        clientId: executionInfo.Data.ClientId,
                        accountId: executionInfo.Data.AccountId,
                        amount: executionInfo.Data.Amount), 
                    _contextNames.TradingEngine);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Unfreezing margin succeded, withdrawal completed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(UnfreezeMarginSucceededWithdrawalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.UnfreezingAmount, State.Succeeded))
            {
                sender.SendCommand(
                    new CompleteWithdrawalInternalCommand(
                        operationId: e.OperationId,
                        clientId: executionInfo.Data.ClientId,
                        accountId: executionInfo.Data.AccountId,
                        amount: executionInfo.Data.Amount), 
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            } 
        }

        /// <summary>
        /// Failed to change balance => withdrawal failed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(UnfreezeMarginOnFailSucceededWithdrawalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DepositData>(OperationName, e.OperationId);
            if (SwitchState(executionInfo.Data, State.UnfreezingAmount, State.Succeeded))
            {
                sender.SendCommand(
                    new FailWithdrawalInternalCommand(e.OperationId, "Failed to change balance."), 
                    _contextNames.AccountsManagement);
                _chaosKitty.Meow(e.OperationId);
                await _executionInfoRepository.Save(executionInfo);
            } 
        }

        /// <summary>
        /// Withdrawal failed
        /// </summary>
        [UsedImplicitly]
        private Task Handle(WithdrawalFailedEvent e, ICommandSender sender)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Withdrawal succeeded
        /// </summary>
        [UsedImplicitly]
        private Task Handle(WithdrawalSucceededEvent e, ICommandSender sender)
        {
            return Task.CompletedTask;
        }

        private static bool SwitchState(DepositData data, State expectedState, State nextState)
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

        private class DepositData
        {
            public string ClientId { get; set; }
            public string AccountId { get; set; }
            public decimal Amount { get; set; }
            public string AuditLog { get; set; }
            public State State { get; set; }
            public string Comment { get; set; }

            [CanBeNull]
            public string FailReason { get; set; }
        }

        private enum State
        {
            FreezingAmount = 1,
            UpdatingBalance = 2,
            UnfreezingAmount = 3,
            Succeeded = 4,
            Failed = 5,
        }
    }
}