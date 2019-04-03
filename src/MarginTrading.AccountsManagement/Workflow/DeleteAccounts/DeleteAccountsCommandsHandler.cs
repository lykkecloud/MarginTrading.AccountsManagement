using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Commands;
using MarginTrading.AccountsManagement.Workflow.DeleteAccounts.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.DeleteAccounts
{
    internal class DeleteAccountsCommandsHandler
    {
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;
        private readonly IEventSender _eventSender;
        private readonly AccountManagementSettings _settings;
        
        private string OperationName => DeleteAccountsSaga.OperationName;
        
        public DeleteAccountsCommandsHandler(
            IOperationExecutionInfoRepository executionInfoRepository,
            IAccountsRepository accountsRepository,
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            ILog log,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            IConvertService convertService,
            IEventSender eventSender,
            AccountManagementSettings settings)
        {
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _log = log;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _convertService = convertService;
            _eventSender = eventSender;
            _settings = settings;
        }

        /// <summary>
        /// Handles the command to begin the accounts deletion process
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DeleteAccountsCommand command, IEventPublisher publisher)
        {
            if (string.IsNullOrWhiteSpace(command.OperationId))
            {
                command.OperationId = Guid.NewGuid().ToString("N");
            }

            if (command.AccountIds == null || !command.AccountIds.Any())
            {
                publisher.PublishEvent(new AccountsDeletionFinishedEvent(
                    operationId: command.OperationId,
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    deletedAccountIds: new List<string>(),
                    failedAccounts: new Dictionary<string, string>(),
                    comment: command.Comment
                ));
                return;
            }

            command.AccountIds = command.AccountIds.Distinct().ToList();
            
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<DeleteAccountsData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    data: new DeleteAccountsData
                    {
                        State = DeleteAccountsState.Initiated,
                        OperationId = command.OperationId,
                        AccountIds = command.AccountIds,
                        Comment = command.Comment,
                    },
                    lastModified: _systemClock.UtcNow.UtcDateTime));

            if (executionInfo.Data.State != DeleteAccountsState.Initiated)
            {
                return;
            }

            var failedAccounts = await ValidateAccountsAsync(executionInfo.Data.AccountIds);
            
            foreach (var accountToBlock in command.AccountIds.Except(failedAccounts.Keys))
            {
                try
                {
                    var account = (await _accountsRepository.GetAsync(accountToBlock))
                        .RequiredNotNull(nameof(accountToBlock), $"Account {accountToBlock} does not exist.");

                    var result = await _accountsRepository.UpdateAccountAsync(
                        accountToBlock,
                        account.TradingConditionId,
                        true,
                        true);
            
                    _eventSender.SendAccountChangedEvent(
                        nameof(DeleteAccountsCommand),
                        result,
                        AccountChangedEventTypeContract.Updated,
                        $"{command.OperationId}_{accountToBlock}",
                        previousSnapshot: account);
                }
                catch (Exception exception)
                {
                    await _log.WriteErrorAsync(nameof(DeleteAccountsCommandsHandler), 
                        nameof(DeleteAccountsCommand), exception);
                    failedAccounts.Add(accountToBlock, exception.Message);
                }
            }

            if (!command.AccountIds.Except(failedAccounts.Keys).Any())
            {
                publisher.PublishEvent(new AccountsDeletionFinishedEvent(
                    operationId: command.OperationId,
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    deletedAccountIds: new List<string>(),
                    failedAccounts: failedAccounts,
                    comment: executionInfo.Data.Comment
                ));
                return;
            }

            _chaosKitty.Meow($"{nameof(DeleteAccountsCommand)}: " +
                "DeleteAccountsStartedInternalEvent: " +
                $"{command.OperationId}");

            publisher.PublishEvent(new DeleteAccountsStartedInternalEvent(
                operationId: command.OperationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                failedAccountIds: failedAccounts
            ));
        }

        /// <summary>
        /// Handles the command to validate one more time and mark accounts as deleted or fail accounts deletion
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(MarkAccountsAsDeletedInternalCommand command, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(OperationName, 
                command.OperationId);

            if (executionInfo == null || executionInfo.Data.State > DeleteAccountsState.MtCoreAccountsBlocked)
            {
                return;
            }
            if (executionInfo.Data.State < DeleteAccountsState.MtCoreAccountsBlocked)
            {
                throw new Exception($"{nameof(MarkAccountsAsDeletedInternalCommand)} have state {executionInfo.Data.State.ToString()}, but [{DeleteAccountsState.MtCoreAccountsBlocked}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }

            var validationFailedAccountIds = await ValidateAccountsAsync(executionInfo.Data.AccountIds);

            foreach (var accountToDelete in executionInfo.Data.GetAccountsToDelete()
                .Except(validationFailedAccountIds.Keys))
            {
                try
                {
                    var account = await _accountsRepository.DeleteAsync(accountToDelete);
                    
                    publisher.PublishEvent(
                        new AccountChangedEvent(
                            account.ModificationTimestamp,
                            OperationName,
                            _convertService.Convert<IAccount, AccountContract>(account),
                            AccountChangedEventTypeContract.Deleted,
                            null,
                            command.OperationId,
                            null));
                }
                catch (Exception exception)
                {
                    await _log.WriteErrorAsync(nameof(DeleteAccountsCommandsHandler), nameof(MarkAccountsAsDeletedInternalCommand),
                        $"OperationId: [{command.OperationId}]", exception);
                    validationFailedAccountIds.Add(accountToDelete, exception.Message);
                }
            }

            foreach (var failedAccountId in executionInfo.Data.FailedAccountIds.Keys
                .Concat(validationFailedAccountIds.Keys))
            {
                try
                {
                    var account = (await _accountsRepository.GetAsync(failedAccountId))
                        .RequiredNotNull(nameof(failedAccountId), $"Account {failedAccountId} does not exist.");

                    var result = await _accountsRepository.UpdateAccountAsync(
                        failedAccountId,
                        account.TradingConditionId,
                        false,
                        false);
            
                    _eventSender.SendAccountChangedEvent(
                        nameof(MarkAccountsAsDeletedInternalCommand),
                        result,
                        AccountChangedEventTypeContract.Updated,
                        $"{command.OperationId}_{failedAccountId}",
                        previousSnapshot: account);
                }
                catch (Exception exception)
                {
                    await _log.WriteErrorAsync(nameof(DeleteAccountsCommandsHandler), 
                        nameof(MarkAccountsAsDeletedInternalCommand), exception);
                }
            }
            
            _chaosKitty.Meow($"{nameof(MarkAccountsAsDeletedInternalCommand)}: " +
                             "AccountsMarkedAsDeletedEvent: " +
                             $"{command.OperationId}");

            publisher.PublishEvent(new AccountsMarkedAsDeletedEvent(
                operationId: command.OperationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                validationFailedAccountIds: validationFailedAccountIds
            ));
        }

        /// <summary>
        /// Handles the command to finish accounts deletion
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(FinishAccountsDeletionInternalCommand command, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DeleteAccountsData>(OperationName, 
                command.OperationId);

            if (executionInfo == null || executionInfo.Data.State > DeleteAccountsState.Finished)
            {
                return;
            }
            if (executionInfo.Data.State < DeleteAccountsState.Finished)
            {
                throw new Exception($"{nameof(FinishAccountsDeletionInternalCommand)} have state {executionInfo.Data.State.ToString()}, but [{DeleteAccountsState.Finished}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            _chaosKitty.Meow($"{nameof(FinishAccountsDeletionInternalCommand)}: " +
                             "AccountsDeletionFinishedEvent: " +
                             $"{command.OperationId}");

            publisher.PublishEvent(new AccountsDeletionFinishedEvent(
                operationId: command.OperationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                deletedAccountIds: executionInfo.Data.GetAccountsToDelete(),
                failedAccounts: executionInfo.Data.FailedAccountIds,
                comment: executionInfo.Data.Comment
            ));
        }

        /// <summary>
        /// Validate accounts for deletion.
        /// </summary>
        /// <param name="accountIdsToValidate">Accounts to validate.</param>
        /// <returns>Dictionary of failed accountIds with fail reason.</returns>
        private async Task<Dictionary<string, string>> ValidateAccountsAsync(
            IEnumerable<string> accountIdsToValidate)
        {
            var failedAccounts = new Dictionary<string, string>();
            
            foreach (var accountId in accountIdsToValidate)
            {
                var account = await _accountsRepository.GetAsync(accountId);

                if (account == null)
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] does not exist");
                    continue;
                }

                if (account.IsDeleted)
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] is deleted. No operations are permitted.");
                    continue;
                }

                if (account.Balance != 0)
                {
                    failedAccounts.Add(accountId, 
                        $"Account [{accountId}] balance is non-zero, so it cannot be deleted.");
                    continue;
                }

                var todayTransactions = await _accountBalanceChangesRepository.GetAsync(accountId, _systemClock.UtcNow.UtcDateTime.Date);
                if (todayTransactions.Any())
                {
                    failedAccounts.Add(accountId, $"Account [{accountId}] had {todayTransactions.Count} transactions today. Please try to delete an account tomorrow.");
                    continue;
                }
            }

            return failedAccounts;
        }
    }
}