using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    [UsedImplicitly]
    internal class EventSender : IEventSender
    {
        private readonly IConvertService _convertService;
        private readonly ISystemClock _systemClock;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly CqrsContextNamesSettings _contextNames;

        public EventSender(
            IConvertService convertService,
            ISystemClock systemClock,
            ICqrsEngine cqrsEngine, CqrsContextNamesSettings contextNames)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _cqrsEngine = cqrsEngine;
            _contextNames = contextNames;
        }

        public void SendAccountChangedEvent(Account account, AccountChangedEventTypeContract eventType)
        {
            _cqrsEngine.PublishEvent(
                new AccountChangedEvent(
                    account.ModificationTimestamp.DateTime,
                    _convertService.Convert<Account, AccountContract>(account),
                    eventType),
                _contextNames.AccountsManagement);
        }
    }
}