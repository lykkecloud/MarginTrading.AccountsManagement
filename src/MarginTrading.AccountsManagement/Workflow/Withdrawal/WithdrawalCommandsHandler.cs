﻿using System;
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
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Events;
using Microsoft.Extensions.Internal;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalCommandsHandler
    {
        private readonly ISystemClock _systemClock;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private const string OperationName = "Withdraw";
        private readonly IChaosKitty _chaosKitty;

        public WithdrawalCommandsHandler(IOperationExecutionInfoRepository executionInfoRepository, 
        ISystemClock systemClock,
        IAccountsRepository accountsRepository, IChaosKitty chaosKitty)
        {
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(WithdrawCommand command, IEventPublisher publisher)
        {
            var executionInfo =  _executionInfoRepository.GetOrAddAsync(
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
            
            var account = _accountsRepository.GetAsync(command.ClientId, command.AccountId).GetAwaiter().GetResult();
            if (account == null || account.Balance < command.Amount)
            {
                publisher.PublishEvent(new WithdrawalStartFailedInternalEvent(command.OperationId,
                    _systemClock.UtcNow.UtcDateTime));
                _chaosKitty.Meow(command.OperationId);
                return;
            }
           
            publisher.PublishEvent(new WithdrawalStartedInternalEvent(command.OperationId, 
                _systemClock.UtcNow.UtcDateTime));
            _chaosKitty.Meow(command.OperationId);
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            
            publisher.PublishEvent(new WithdrawalFailedEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime));
            _chaosKitty.Meow(command.OperationId);
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new WithdrawalSucceededEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime));
            _chaosKitty.Meow(command.OperationId);
        }
    }
}