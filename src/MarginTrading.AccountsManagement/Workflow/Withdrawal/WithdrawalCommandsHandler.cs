using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;
using Microsoft.Extensions.Internal;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalCommandsHandler
    {
        private readonly ISystemClock _systemClock;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        
        private const string OperationName = "Withdraw";

        public WithdrawalCommandsHandler(
            ISystemClock systemClock,
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            IAccountsRepository accountsRepository,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty)
        {
            _systemClock = systemClock;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(WithdrawCommand command, IEventPublisher publisher)
        {
            await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<WithdrawalDepositData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    data: new WithdrawalDepositData
                    {
                        AccountId = command.AccountId,
                        Amount = command.Amount,
                        AuditLog = command.AuditLog,
                        State = State.Created,
                        Comment = command.Comment
                    },
                    lastModified: _systemClock.UtcNow.UtcDateTime));

            var account = await _accountsRepository.GetAsync(command.AccountId);
            var realisedDailyPnl = (await _accountBalanceChangesRepository.GetAsync(
                accountId: command.AccountId,
                //TODO rethink the way trading day's start & end are selected 
                from: _systemClock.UtcNow.UtcDateTime.Date
            )).Where(x => x.ReasonType == AccountBalanceChangeReasonType.RealizedPnL).Sum(x => x.ChangeAmount);
            
            if (account == null || account.Balance - realisedDailyPnl < command.Amount)
            {
                _chaosKitty.Meow(command.OperationId);
                publisher.PublishEvent(new WithdrawalStartFailedInternalEvent(command.OperationId,
                    _systemClock.UtcNow.UtcDateTime, account == null
                        ? $"Account {command.AccountId} not found."
                        : "Account balance is not enough"));
                return;
            }

            if (account.IsWithdrawalDisabled)
            {
                publisher.PublishEvent(new WithdrawalStartFailedInternalEvent(command.OperationId,
                    _systemClock.UtcNow.UtcDateTime, "Withdrawal is disabled"));
                return;
            }
            
            _chaosKitty.Meow(command.OperationId);
          
            publisher.PublishEvent(new WithdrawalStartedInternalEvent(command.OperationId, 
                _systemClock.UtcNow.UtcDateTime));
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(
                OperationName,
                command.OperationId
            );

            if (executionInfo == null)
                return;

            publisher.PublishEvent(new WithdrawalFailedEvent(command.OperationId,
                _systemClock.UtcNow.UtcDateTime, command.Reason));
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<WithdrawalDepositData>(
                OperationName,
                command.OperationId
            );

            if (executionInfo == null)
                return;

            var account = await _accountsRepository.GetAsync(executionInfo.Data.AccountId);

            publisher.PublishEvent(new WithdrawalSucceededEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime,
                account?.ClientId, executionInfo.Data.AccountId, executionInfo.Data.Amount));
        }
    }
}