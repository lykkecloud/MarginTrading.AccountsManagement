﻿using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    internal class UpdateBalanceCommandsHandler
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;

        public UpdateBalanceCommandsHandler(IAccountsRepository accountsRepository,
            IChaosKitty chaosKitty, IConvertService convertService)
        {
            _accountsRepository = accountsRepository;
            _chaosKitty = chaosKitty;
            _convertService = convertService;
        }

        /// <summary>
        /// Handles the command to change the balance
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(UpdateBalanceInternalCommand command,
            IEventPublisher publisher)
        {
            IAccount account = null;
            try
            {
                account = await _accountsRepository.UpdateBalanceAsync(
                    operationId: command.OperationId,
                    clientId: command.ClientId,
                    accountId: command.AccountId,
                    amountDelta: command.AmountDelta,
                    changeLimit: false);
            }
            catch (Exception ex)
            {
                publisher.PublishEvent(new AccountBalanceChangeFailedEvent(command.OperationId, ex.Message));
                return CommandHandlingResult.Ok(); //means no retries required
            }

            _chaosKitty.Meow(command.OperationId);

            var change = new AccountBalanceChangeContract(
                id: command.OperationId,
                changeTimestamp: account.ModificationTimestamp,
                accountId: account.Id,
                clientId: account.ClientId,
                changeAmount: command.AmountDelta,
                balance: account.Balance,
                withdrawTransferLimit: account.WithdrawTransferLimit,
                comment: command.Comment,
                reasonType: Convert(command.ChangeReasonType),
                eventSourceId: command.Source,
                legalEntity: account.LegalEntity,
                auditLog: command.AuditLog,
                instrument: null,//TODO pass through ClosePositionSaga from MT Core
                tradingDate: DateTime.UtcNow);//TODO pass from API call

            var convertedAccount = Convert(account);

            publisher.PublishEvent(new AccountChangedEvent(change.ChangeTimestamp, command.Source, convertedAccount,
                AccountChangedEventTypeContract.BalanceUpdated, change));
            
            return CommandHandlingResult.Ok();
        }

        private AccountContract Convert(IAccount account)
        {
            return _convertService.Convert<AccountContract>(account);
        }

        private AccountBalanceChangeReasonTypeContract Convert(AccountBalanceChangeReasonType reasonType)
        {
            return _convertService.Convert<AccountBalanceChangeReasonTypeContract>(reasonType);
        }
    }
}