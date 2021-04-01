// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        public NegativeProtectionSaga(
            CqrsContextNamesSettings contextNames,
            INegativeProtectionService negativeProtectionService,
            IAccountsRepository accountsRepository,
            ISystemClock systemClock)
        {
            _contextNames = contextNames;
            _negativeProtectionService = negativeProtectionService;
            _accountsRepository = accountsRepository;
            _systemClock = systemClock;
        }

        [UsedImplicitly]
        private async Task Handle(AccountChangedEvent evt, ICommandSender sender)
        {
            if (evt.EventType != AccountChangedEventTypeContract.BalanceUpdated)
                return;
            if (evt.BalanceChange == null)
                return;
            
            var account = await _accountsRepository.GetAsync(evt.Account.Id);
            var amount = await _negativeProtectionService.CheckAsync(evt.OperationId, evt.Account.Id, evt.BalanceChange.Balance);

            if (account == null || amount == null)
            {
                return;
            }
            
            sender.SendCommand(new NotifyNegativeProtectionInternalCommand(
                    Guid.NewGuid().ToString("N"),
                    evt.OperationId,
                    evt.OperationId,
                    _systemClock.UtcNow.UtcDateTime,
                    account.ClientId,
                    account.Id,
                    amount.Value
                ),
                _contextNames.AccountsManagement);
        }
    }
}