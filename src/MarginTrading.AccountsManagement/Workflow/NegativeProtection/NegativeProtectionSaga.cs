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
using MarginTrading.Backend.Contracts.Workflow.Liquidation.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.NegativeProtection
{
    internal class NegativeProtectionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly INegativeProtectionService _negativeProtectionService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ISystemClock _systemClock;

        public NegativeProtectionSaga(CqrsContextNamesSettings contextNames,
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
        private async Task Handle(LiquidationFinishedEvent evt, ICommandSender sender)
        {
            await Handle(evt.OperationId, evt.AccountId,
                evt.OpenPositionsRemainingOnAccount, evt.CurrentTotalCapital, sender);
        }

        [UsedImplicitly]
        private async Task Handle(LiquidationFailedEvent evt, ICommandSender sender)
        {
            await Handle(evt.OperationId, evt.AccountId,
                evt.OpenPositionsRemainingOnAccount, evt.CurrentTotalCapital, sender);
        }
        
        private async Task Handle(string operationId, string accountId, 
            int openPositionsOnAccount, decimal currentTotalCapital, ICommandSender sender)
        {
            var account = await _accountsRepository.GetAsync(accountId);
            var amount = await _negativeProtectionService.CheckAsync(operationId, account, currentTotalCapital);

            if (account == null || amount == null)
            {
                return;
            }

            sender.SendCommand(new NotifyNegativeProtectionInternalCommand(
                    id: Guid.NewGuid().ToString("N"),
                    correlationId: operationId,
                    causationId: operationId,
                    eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                    clientId: account.ClientId,
                    accountId: account.Id,
                    amount: amount.Value,
                    openPositionsRemainingOnAccount: openPositionsOnAccount,
                    currentTotalCapital: currentTotalCapital
                ),
                _contextNames.AccountsManagement);
        }
    }
}