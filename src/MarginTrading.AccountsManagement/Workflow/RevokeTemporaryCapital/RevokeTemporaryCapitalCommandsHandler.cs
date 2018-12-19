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
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital
{
    internal class RevokeTemporaryCapitalCommandsHandler
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly AccountManagementSettings _settings;
        
        private const string OperationName = RevokeTemporaryCapitalSaga.OperationName;

        public RevokeTemporaryCapitalCommandsHandler(
            IAccountsRepository accountsRepository,
            ISystemClock systemClock,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty,
            ILog log,
            AccountManagementSettings settings)
        {
            _accountsRepository = accountsRepository;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
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
                operationName: OperationName,
                operationId: c.OperationId,
                factory: () => new OperationExecutionInfo<RevokeTemporaryCapitalData>(
                    operationName: OperationName,
                    id: c.OperationId,
                    data: new RevokeTemporaryCapitalData 
                    {
                        State = TemporaryCapitalState.Initiated,
                        OperationId = c.OperationId,
                        EventSourceId = c.EventSourceId,
                        AccountId = c.AccountId,
                        RevokeEventSourceId = c.RevokeEventSourceId,
                        AuditLog = c.AuditLog,
                    },
                    lastModified: _systemClock.UtcNow.UtcDateTime));

            if (executionInfo.Data.State != TemporaryCapitalState.Initiated)
            {
                return;
            }

            _chaosKitty.Meow(c.OperationId);
            
            var account = await _accountsRepository.GetAsync(c.AccountId);

            if (account == null)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime, $"Account {c.AccountId} not found", c.EventSourceId,
                    c.RevokeEventSourceId));
                return;
            }

            if (account.IsDisabled)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, $"Account {c.AccountId} is disabled", c.EventSourceId,
                    c.RevokeEventSourceId));
                return;
            }

            if (!string.IsNullOrEmpty(c.RevokeEventSourceId) 
                && account.TemporaryCapital.All(x => x.Id != c.RevokeEventSourceId))
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    $"Account {c.AccountId} doesn't contain temporary capital with id {c.RevokeEventSourceId}",
                    c.EventSourceId, c.RevokeEventSourceId));
                return; 
            }

            var temporaryCapitalToRevoke = account.TemporaryCapital
                .Where(x => string.IsNullOrEmpty(c.RevokeEventSourceId) || x.Id == c.RevokeEventSourceId)
                .ToList();
            
            //todo ask Anton
//            var amountToRevoke = temporaryCapitalToRevoke.Select(x => x.Amount).Sum();
//            if (account.Balance < amountToRevoke)
//            {
//                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId,
//                    _systemClock.UtcNow.UtcDateTime,
//                    $"Account {c.AccountId} balance {account.Balance}{account.BaseAssetId} is not enough to revoke {amountToRevoke}{account.BaseAssetId}",
//                    c.EventSourceId, c.RevokeEventSourceId));
//                return; 
//            }

            try
            {
                await _accountsRepository.UpdateAccountTemporaryCapitalAsync(c.AccountId,
                    string.IsNullOrEmpty(c.RevokeEventSourceId)
                    ? null
                    : new InternalModels.TemporaryCapital { Id = c.RevokeEventSourceId },
                    addOrRemove: false);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(RevokeTemporaryCapitalCommandsHandler),
                    nameof(StartRevokeTemporaryCapitalInternalCommand), exception);
                
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, exception.Message, c.EventSourceId, 
                    c.RevokeEventSourceId));
                
                return;
            }

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
            
            _chaosKitty.Meow(c.OperationId);

            if (!new[] {TemporaryCapitalState.ChargedOnAccount, TemporaryCapitalState.Failing}
                .Contains(executionInfo.Data.State))
            {
                throw new Exception($"{nameof(FinishRevokeTemporaryCapitalInternalCommand)} have state {executionInfo.Data.State.ToString()}, but one of [{TemporaryCapitalState.ChargedOnAccount}, {TemporaryCapitalState.Failing}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            if (executionInfo.Data.State == TemporaryCapitalState.ChargedOnAccount)
            {
                publisher.PublishEvent(new RevokeTemporaryCapitalSucceededEvent(
                    operationId: c.OperationId, 
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    eventSourceId: executionInfo.Data.EventSourceId,
                    accountId: executionInfo.Data.AccountId,
                    revokeEventSourceId: executionInfo.Data.RevokeEventSourceId,
                    auditLog: executionInfo.Data.AuditLog));
            }
            else if (executionInfo.Data.State == TemporaryCapitalState.Failing)
            {
                //rollback account. if exception is thrown here, it will be retried until success
                await _accountsRepository.UpdateAccountRollbackTemporaryCapitalAsync(executionInfo.Data.AccountId,
                    executionInfo.Data.RevokedTemporaryCapital);
                
                publisher.PublishEvent(new RevokeTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, executionInfo.Data.FailReason, executionInfo.Data.EventSourceId,
                    executionInfo.Data.RevokeEventSourceId));
            }
        }
    }
}