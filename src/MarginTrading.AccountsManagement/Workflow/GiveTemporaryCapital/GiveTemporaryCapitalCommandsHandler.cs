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
using MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital.Events;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.RevokeTemporaryCapital.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.GiveTemporaryCapital
{
    internal class GiveTemporaryCapitalCommandsHandler
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly AccountManagementSettings _settings;
        
        private const string OperationName = GiveTemporaryCapitalSaga.OperationName;

        public GiveTemporaryCapitalCommandsHandler(
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
        /// Handles the command to begin giving temporary capital
        /// </summary>
        [UsedImplicitly]
        public async Task Handle(StartGiveTemporaryCapitalInternalCommand c, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: c.OperationId,
                factory: () => new OperationExecutionInfo<GiveTemporaryCapitalData>(
                    operationName: OperationName,
                    id: c.OperationId,
                    data: new GiveTemporaryCapitalData 
                    {
                        State = TemporaryCapitalState.Initiated,
                        OperationId = c.OperationId,
                        EventSourceId = c.EventSourceId,
                        AccountId = c.AccountId,
                        Amount = c.Amount,
                        Reason = c.Reason,
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
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, $"Account {c.AccountId} not found", c.EventSourceId));
                return;
            }

            if (account.IsDisabled)
            {
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, $"Account {c.AccountId} is disabled", c.EventSourceId));
                return;
            }

            if (account.TemporaryCapital.Any(x => x.Id == c.EventSourceId))
            {
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    $"Account {c.AccountId} already contain temporary capital with id {c.EventSourceId}",
                    c.EventSourceId));
                return; 
            }

            try
            {
                await _accountsRepository.UpdateAccountTemporaryCapitalAsync(c.AccountId,
                    new InternalModels.TemporaryCapital
                    {
                        Id = c.EventSourceId,
                        Amount = c.Amount,
                    },
                    addOrRemove: true);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(GiveTemporaryCapitalCommandsHandler),
                    nameof(StartGiveTemporaryCapitalInternalCommand), exception);
                
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, exception.Message, c.EventSourceId));
                
                return;
            }

            publisher.PublishEvent(new GiveTemporaryCapitalStartedInternalEvent(c.OperationId, 
                _systemClock.UtcNow.UtcDateTime));
        }

        /// <summary>
        /// Give temporary capital operation is over, finishing it
        /// </summary>
        [UsedImplicitly]
        public async Task Handle(FinishGiveTemporaryCapitalInternalCommand c, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(OperationName, c.OperationId);

            if (executionInfo == null)
                return;
            
            _chaosKitty.Meow(c.OperationId);

            if (!new[] {TemporaryCapitalState.ChargedOnAccount, TemporaryCapitalState.Failing}
                .Contains(executionInfo.Data.State))
            {
                throw new Exception($"{nameof(FinishGiveTemporaryCapitalInternalCommand)} have state {executionInfo.Data.State.ToString()}, but one of [{TemporaryCapitalState.ChargedOnAccount}, {TemporaryCapitalState.Failing}] was expected. Throwing to retry in {(long) _settings.Cqrs.RetryDelay.TotalMilliseconds}ms.");
            }
            
            if (executionInfo.Data.State == TemporaryCapitalState.ChargedOnAccount)
            {
                publisher.PublishEvent(new GiveTemporaryCapitalSucceededEvent(
                    operationId: c.OperationId, 
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    eventSourceId: executionInfo.Data.EventSourceId,
                    accountId: executionInfo.Data.AccountId,
                    amount: executionInfo.Data.Amount,
                    reason: executionInfo.Data.Reason,
                    auditLog: executionInfo.Data.AuditLog));
            }
            else if (executionInfo.Data.State == TemporaryCapitalState.Failing)
            {
                //rollback account. if exception is thrown here, it will be retried until success
                await _accountsRepository.UpdateAccountTemporaryCapitalAsync(executionInfo.Data.AccountId,
                    new InternalModels.TemporaryCapital
                    {
                        Id = executionInfo.Data.EventSourceId,
                        Amount = executionInfo.Data.Amount,
                    },
                    addOrRemove: false);
                
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, executionInfo.Data.FailReason, executionInfo.Data.EventSourceId));
            }
        }
    }
}