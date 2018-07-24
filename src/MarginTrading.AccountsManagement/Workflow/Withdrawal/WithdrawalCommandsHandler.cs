using System;
using AutoMapper;
using JetBrains.Annotations;
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

        public WithdrawalCommandsHandler(IOperationExecutionInfoRepository executionInfoRepository, 
        ISystemClock systemClock,
        IAccountsRepository accountsRepository)
        {
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
        }

        /// <summary>
        /// Handles the command to begin the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(WithdrawCommand command, IEventPublisher publisher)
        {
            
            var account = _accountsRepository.GetAsync(command.ClientId, command.AccountId).GetAwaiter().GetResult();
            if (account == null || account.Balance < command.Amount)
            {
                var executionInfoForFailedEvent =
                    new OperationExecutionInfo<WithdrawalSaga.DepositData>(OperationName, command.OperationId,
                        new WithdrawalSaga.DepositData()
                        {
                            ClientId = command.ClientId,
                            AccountId = command.AccountId,
                            Amount = command.Amount,
                            AuditLog = command.AuditLog,
                            Comment = command.Comment,
                            State = WithdrawalSaga.State.Failed

                        });

                _executionInfoRepository.Save(executionInfoForFailedEvent);

                publisher.PublishEvent(new WithdrawalStartFailedInternalEvent(command.OperationId,
                    _systemClock.UtcNow.UtcDateTime, command.ClientId, command.AccountId, command.Amount,
                    "Not enough balance for withdrawal"));
                return;
            }
            var executionInfo =
                new OperationExecutionInfo<WithdrawalSaga.DepositData>(OperationName, command.OperationId,
                    new WithdrawalSaga.DepositData()
                    {
                        ClientId = command.ClientId,
                        AccountId = command.AccountId,
                        Amount = command.Amount,
                        AuditLog = command.AuditLog,
                        Comment = command.Comment,
                        State = WithdrawalSaga.State.FreezingAmount

                    });

            _executionInfoRepository.Save(executionInfo);

            publisher.PublishEvent(new WithdrawalStartedInternalEvent(command.OperationId, 
                _systemClock.UtcNow.UtcDateTime, command.ClientId, command.AccountId, command.Amount, command.Comment, 
                command.AuditLog));
        }

        /// <summary>
        /// Handles the command to fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(FailWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new WithdrawalFailedEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime, 
                command.Reason));
        }

        /// <summary>
        /// Handles the command to complete the withdrawal
        /// </summary>
        [UsedImplicitly]
        private void Handle(CompleteWithdrawalInternalCommand command, IEventPublisher publisher)
        {
            publisher.PublishEvent(new WithdrawalSucceededEvent(command.OperationId, _systemClock.UtcNow.UtcDateTime, 
                command.ClientId, command.AccountId, command.Amount));
        }
    }
}