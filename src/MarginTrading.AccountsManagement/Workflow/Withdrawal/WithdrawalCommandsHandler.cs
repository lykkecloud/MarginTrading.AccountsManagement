using System;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
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
        private readonly IAccountManagementService _accountManagementService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        
        private const string OperationName = "Withdraw";

        public WithdrawalCommandsHandler(
            ISystemClock systemClock,
            IAccountManagementService accountManagementService,
            IAccountsRepository accountsRepository,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty)
        {
            _systemClock = systemClock;
            _accountManagementService = accountManagementService;
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
                        ClientId = command.ClientId,
                        AccountId = command.AccountId,
                        Amount = command.Amount,
                        AuditLog = command.AuditLog,
                        State = State.Created,
                        Comment = command.Comment
                    }));

            var account = await _accountsRepository.GetAsync(command.AccountId);
            var accountStat = await _accountManagementService.GetStat(command.AccountId);
            if (account == null || account.Balance - accountStat.RealisedPnl < command.Amount)
            {
                _chaosKitty.Meow(command.OperationId);
                publisher.PublishEvent(new WithdrawalStartFailedInternalEvent(command.OperationId,
                    _systemClock.UtcNow.UtcDateTime, account == null
                        ? $"Account {command.AccountId} not found."
                        : "Account balance is not enough"));
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

            publisher.PublishEvent(new WithdrawalFailedEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime,
                executionInfo.Data.FailReason));
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

            publisher.PublishEvent(new WithdrawalSucceededEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime,
                executionInfo.Data.ClientId, executionInfo.Data.AccountId, executionInfo.Data.Amount));
        }
    }
}