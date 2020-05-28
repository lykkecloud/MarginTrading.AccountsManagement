// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services.Implementation;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Events;
using MarginTrading.Backend.Contracts;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital
{
    internal class RevokeTemporaryCapitalCommandsHandler
    {
        private readonly IAccountsApi _accountsApi;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IAccountBalanceChangesRepository _accountBalanceChangesRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly AccountManagementSettings _settings;
        
        private const string OperationName = RevokeTemporaryCapitalSaga.OperationName;

        public RevokeTemporaryCapitalCommandsHandler(
            IAccountsApi accountsApi,
            IAccountsRepository accountsRepository,
            ISystemClock systemClock,
            IOperationExecutionInfoRepository executionInfoRepository,
            IAccountBalanceChangesRepository accountBalanceChangesRepository,
            IChaosKitty chaosKitty,
            ILog log,
            AccountManagementSettings settings)
        {
            _accountsApi = accountsApi;
            _accountsRepository = accountsRepository;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _accountBalanceChangesRepository = accountBalanceChangesRepository;
            _chaosKitty = chaosKitty;
            _log = log;
            _settings = settings;
        }
        
        /// <summary>
        /// Handles the command to begin revoking temporary capital
        /// </summary>
        [UsedImplicitly]
        public async Task Handle(StartRevokeTemporaryCapitalInternalCommand c, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                OperationName,
                c.OperationId,
                () => new OperationExecutionInfo<RevokeTemporaryCapitalData>(
                    OperationName,
                    c.OperationId,
                    new RevokeTemporaryCapitalData 
                    {
                        State = TemporaryCapitalState.Initiated,
                        OperationId = c.OperationId,
                        AccountId = c.AccountId,
                        RevokeEventSourceId = c.RevokeEventSourceId,
                        Comment = c.Comment,
                        AdditionalInfo = c.AdditionalInfo,
                    },
                    _systemClock.UtcNow.UtcDateTime));

            if (executionInfo.Data.State != TemporaryCapitalState.Initiated)
            {
                return;
            }
            
            var account = await _accountsRepository.GetAsync(c.AccountId);

            if (account == null)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime, $"Account {c.AccountId} not found", 
                    c.RevokeEventSourceId));
                return;
            }

            if (!string.IsNullOrEmpty(c.RevokeEventSourceId) 
                && account.TemporaryCapital.All(x => x.Id != c.RevokeEventSourceId))
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    $"Account {c.AccountId} doesn't contain temporary capital with id {c.RevokeEventSourceId}",
                    c.RevokeEventSourceId));
                return; 
            }

            var temporaryCapitalToRevoke = account.TemporaryCapital
                .Where(x => string.IsNullOrEmpty(c.RevokeEventSourceId) || x.Id == c.RevokeEventSourceId)
                .ToList();
            
            var accountableTransactionsSumForToday = await _accountBalanceChangesRepository.GetRealizedPnlAndCompensationsForToday(c.AccountId);
            var amountToRevoke = temporaryCapitalToRevoke.Select(x => x.Amount).Sum();
            if (account.Balance - accountableTransactionsSumForToday < amountToRevoke)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    $"Account {c.AccountId} balance {account.Balance}{account.BaseAssetId} is not enough to revoke {amountToRevoke}{account.BaseAssetId}."
                    + (accountableTransactionsSumForToday > 0 ? $" Taking into account the sum of the current realized daily PnL and compensation payments {accountableTransactionsSumForToday}{account.BaseAssetId}." : ""),
                    c.RevokeEventSourceId));
                return;
            }

            var accountStat = await _accountsApi.GetAccountStats(c.AccountId);
            if (accountStat != null && accountStat.FreeMargin < amountToRevoke)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    $"MT Core account {c.AccountId} free margin {accountStat.FreeMargin}{account.BaseAssetId} is not enough to revoke {amountToRevoke}{account.BaseAssetId}.",
                    c.RevokeEventSourceId));
                return;
            }

            try
            {
                await _accountsRepository.UpdateAccountTemporaryCapitalAsync(c.AccountId, 
                    AccountManagementService.UpdateTemporaryCapital,
                    string.IsNullOrEmpty(c.RevokeEventSourceId)
                        ? null
                        : new InternalModels.TemporaryCapital { Id = c.RevokeEventSourceId },
                    false);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(RevokeTemporaryCapitalCommandsHandler),
                    nameof(StartRevokeTemporaryCapitalInternalCommand), exception);
                
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, exception.Message, c.RevokeEventSourceId));
                
                return;
            }

            _chaosKitty.Meow(
                $"{nameof(StartRevokeTemporaryCapitalInternalCommand)}: " +
                "publisher.PublishEvent: " +
                $"{c.OperationId}");

            publisher.PublishEvent(new RevokeTemporaryCapitalStartedInternalEvent(c.OperationId, 
                _systemClock.UtcNow.UtcDateTime, temporaryCapitalToRevoke));
        }
        
        /// <summary>
        /// Revoke temporary capital operation is over, finishing it
        /// </summary>
        [UsedImplicitly]
        public async Task Handle(FinishRevokeTemporaryCapitalInternalCommand c, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<RevokeTemporaryCapitalData>(OperationName, c.OperationId);

            if (executionInfo == null)
                return;

            if (!new[] {TemporaryCapitalState.ChargedOnAccount, TemporaryCapitalState.Failing}
                .Contains(executionInfo.Data.State))
            {
                throw new Exception($"{nameof(FinishRevokeTemporaryCapitalInternalCommand)} have state {executionInfo.Data.State.ToString()}, but one of [{TemporaryCapitalState.ChargedOnAccount}, {TemporaryCapitalState.Failing}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            if (executionInfo.Data.State == TemporaryCapitalState.ChargedOnAccount)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalSucceededEvent(
                    c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime,
                    executionInfo.Data.EventSourceId,
                    executionInfo.Data.AccountId,
                    executionInfo.Data.RevokeEventSourceId,
                    executionInfo.Data.Comment,
                    executionInfo.Data.AdditionalInfo));
            }
            else if (executionInfo.Data.State == TemporaryCapitalState.Failing)
            {
                //rollback account. if exception is thrown here, it will be retried until success
                await _accountsRepository.RollbackTemporaryCapitalRevokeAsync(executionInfo.Data.AccountId,
                    executionInfo.Data.RevokedTemporaryCapital);

                _chaosKitty.Meow(
                    $"{nameof(FinishRevokeTemporaryCapitalInternalCommand)}: " +
                    "publisher.PublishEvent: " +
                    $"{c.OperationId}");
                
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, executionInfo.Data.FailReason, 
                    executionInfo.Data.RevokeEventSourceId));
            }
        }
    }
}