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
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Commands;
using MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.TemporaryCapital
{
    internal class TemporaryCapitalCommandsHandler
    {
        private readonly IAccountManagementService _accountManagementService;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        
        private const string OperationName = TemporaryCapitalSaga.OperationName;

        public TemporaryCapitalCommandsHandler(
            IAccountManagementService accountManagementService,
            ISystemClock systemClock,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty,
            ILog log)
        {
            _accountManagementService = accountManagementService;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
            _log = log;
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
                        State = GiveTemporaryCapitalState.Initiated,
                        OperationId = c.OperationId,
                        EventSourceId = c.EventSourceId,
                        AccountId = c.AccountId,
                        Amount = c.Amount,
                        Reason = c.Reason,
                        AuditLog = c.AuditLog,
                    },
                    lastModified: _systemClock.UtcNow.UtcDateTime));

            if (executionInfo.Data.State != GiveTemporaryCapitalState.Initiated)
            {
                return;
            }

            _chaosKitty.Meow(c.OperationId);
            
            var account = await _accountManagementService.GetByIdAsync(c.AccountId);

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
                await _accountManagementService.UpdateAccountTemporaryCapitalAsync(c.AccountId,
                    new InternalModels.TemporaryCapital
                    {
                        Id = c.EventSourceId,
                        Amount = c.Amount,
                    },
                    addOrRemove: true);
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(TemporaryCapitalCommandsHandler),
                    nameof(StartGiveTemporaryCapitalInternalCommand), exception);
                publisher.PublishEvent(new GiveTemporaryCapitalFailedEvent(c.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, $"Failed to write temporary capital on account {c.AccountId}", c.EventSourceId));
                return;
            }

            publisher.PublishEvent(new GiveTemporaryCapitalStartedInternalEvent(c.OperationId, _systemClock.UtcNow.UtcDateTime));
        }
    }
}