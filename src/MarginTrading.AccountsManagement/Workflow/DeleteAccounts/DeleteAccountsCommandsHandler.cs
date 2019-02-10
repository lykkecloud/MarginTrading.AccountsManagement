using System;
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
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
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
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;
        private readonly AccountManagementSettings _settings;
        
        private string OperationName => DeleteAccountsSaga.OperationName;
        
        public DeleteAccountsCommandsHandler(
            IOperationExecutionInfoRepository executionInfoRepository,
            IAccountsRepository accountsRepository,
            ILog log,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            IConvertService convertService,
            AccountManagementSettings settings)
        {
            _executionInfoRepository = executionInfoRepository;
            _accountsRepository = accountsRepository;
            _log = log;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _convertService = convertService;
            _settings = settings;
        }

        /// <summary>
        /// Handles the command to begin the accounts deletion process
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DeleteAccountsCommand command, IEventPublisher publisher)
        {
            if (string.IsNullOrEmpty(command.OperationId))
            {
                command.OperationId = Guid.NewGuid().ToString("N");
            }
            
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

            _chaosKitty.Meow($"{nameof(DeleteAccountsCommand)}: " +
                "Save_OperationExecutionInfo: " +
                $"{command.OperationId}");

            publisher.PublishEvent(new DeleteAccountsStartedInternalEvent(
                operationId: command.OperationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime
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

            if (executionInfo == null)
                return;

            if (executionInfo.Data.State != DeleteAccountsState.MtCoreAccountsBlocked)
            {
                throw new Exception($"{nameof(MarkAccountsAsDeletedInternalCommand)} have state {executionInfo.Data.State.ToString()}, but [{DeleteAccountsState.MtCoreAccountsBlocked}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }

            var validationFailedAccountIds = 
                await DeleteAccountsSaga.ValidateAccountsAsync(executionInfo.Data.AccountIds, _accountsRepository);

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
                    _log.Error($"{nameof(DeleteAccountsCommandsHandler)}: {nameof(MarkAccountsAsDeletedInternalCommand)}",
                        exception, $"OperationId: [{command.OperationId}]");
                    validationFailedAccountIds.Add(accountToDelete, exception.Message);
                }
            }
            
            _chaosKitty.Meow($"{nameof(MarkAccountsAsDeletedInternalCommand)}: " +
                             "Save_OperationExecutionInfo: " +
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

            if (executionInfo == null)
                return;

            if (executionInfo.Data.State != DeleteAccountsState.Finished)
            {
                throw new Exception($"{nameof(FinishAccountsDeletionInternalCommand)} have state {executionInfo.Data.State.ToString()}, but [{DeleteAccountsState.Finished}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            _chaosKitty.Meow($"{nameof(FinishAccountsDeletionInternalCommand)}: " +
                             "Save_OperationExecutionInfo: " +
                             $"{command.OperationId}");

            publisher.PublishEvent(new AccountsDeletionFinishedEvent(
                operationId: command.OperationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                deletedAccountIds: executionInfo.Data.GetAccountsToDelete(),
                failedAccountIds: executionInfo.Data.FailedAccountIds,
                comment: executionInfo.Data.Comment
            ));
        }
    }
}