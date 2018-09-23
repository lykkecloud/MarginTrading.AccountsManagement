using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.NegativeProtection.Commands;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.NegativeProtection
{
    internal class NegativeProtectionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly INegativeProtectionService _negativeProtectionService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;

        public NegativeProtectionSaga(CqrsContextNamesSettings contextNames,
            INegativeProtectionService negativeProtectionService,
            IAccountsRepository accountsRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _negativeProtectionService = negativeProtectionService;
            _accountsRepository = accountsRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
        }

        [UsedImplicitly]
        private async Task Handle(AccountChangedEvent evt, ICommandSender sender)
        {
            if (evt.EventType != AccountChangedEventTypeContract.BalanceUpdated || evt.BalanceChange == null)
                return;
            
            var account = await _accountsRepository.GetAsync(evt.Account.Id);

            var correlationId = evt.Source ?? Guid.NewGuid().ToString("N"); //if comes through API;
            var causationId = evt.BalanceChange.Id;
            if (!await _negativeProtectionService.CheckAsync(
                correlationId: correlationId,
                causationId: causationId,
                account: account))
                return;

            sender.SendCommand(
                new NotifyNegativeProtectionInternalCommand(
                    id: Guid.NewGuid().ToString("N"),
                    correlationId: correlationId,
                    causationId: causationId,
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    clientId: evt.Account.ClientId,
                    accountId: evt.Account.Id,
                    amount: Math.Abs(evt.BalanceChange.Balance)
                ),
                _contextNames.AccountsManagement);
            
            _chaosKitty.Meow(causationId);
        }
    }
}