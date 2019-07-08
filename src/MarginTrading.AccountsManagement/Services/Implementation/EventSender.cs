// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        public ICqrsEngine CqrsEngine { get; set; }//property injection
        private readonly CqrsContextNamesSettings _contextNames;

        public EventSender(
            IRabbitMqService rabbitMqService,
            IConvertService convertService,
            ISystemClock systemClock,
            CqrsContextNamesSettings contextNames)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _contextNames = contextNames;
        }

        public void SendAccountChangedEvent(string source, IAccount account, AccountChangedEventTypeContract eventType,
            string operationId, AccountBalanceChangeContract balanceChangeContract = null, 
            IAccount previousSnapshot = null)
        {
            var metadata = previousSnapshot != null
                ? new AccountChangeMetadata
                {
                    PreviousAccountSnapshot =
                        _convertService.Convert<IAccount, AccountContract>(previousSnapshot)
                }
                : null;

            CqrsEngine.PublishEvent(
                new AccountChangedEvent(
                    account.ModificationTimestamp,
                    source,
                    _convertService.Convert<IAccount, AccountContract>(account),
                    eventType,
                    balanceChangeContract,
                    operationId,
                    metadata.ToJson()),
                _contextNames.AccountsManagement);
        }
    }
}