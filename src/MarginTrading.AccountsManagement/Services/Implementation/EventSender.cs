using System;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
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
            IRabbitMqService rabbitMqService,
            IConvertService convertService,
            ISystemClock systemClock,
            ICqrsEngine cqrsEngine,
            CqrsContextNamesSettings contextNames)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _cqrsEngine = cqrsEngine;
            _contextNames = contextNames;
        }

        public void SendAccountChangedEvent(string source, IAccount account, AccountChangedEventTypeContract eventType,
            string operationId, AccountBalanceChangeContract balanceChangeContract = null)
        {
            _cqrsEngine.PublishEvent(
                new AccountChangedEvent(
                    account.ModificationTimestamp,
                    source,
                    _convertService.Convert<IAccount, AccountContract>(account),
                    eventType,
                    balanceChangeContract,
                    operationId),
                _contextNames.AccountsManagement);
        }
    }
}