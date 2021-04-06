// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Common;
using Common.Log;   
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Settings;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    [UsedImplicitly]
    internal class EventSender : IEventSender
    {
        private readonly IConvertService _convertService;
        public ICqrsEngine CqrsEngine { get; set; }//property injection
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly ILog _log;

        public EventSender(
            IConvertService convertService,
            CqrsContextNamesSettings contextNames,
            ILog log)
        {
            _convertService = convertService;
            _contextNames = contextNames;
            _log = log;
        }

        public void SendAccountChangedEvent(string source, IAccount account, AccountChangedEventTypeContract eventType,
            string operationId, AccountBalanceChangeContract balanceChangeContract = null, 
            IAccount previousSnapshot = null,
            string orderId = null)
        {
            _log.WriteInfo(nameof(EventSender), nameof(SendAccountChangedEvent), $"SendAccountChangedEvent: AccountId {account.Id}, OrderId {orderId}");
            var metadata = new AccountChangeMetadata {OrderId = orderId};

            if (previousSnapshot != null)
            {
                metadata.PreviousAccountSnapshot =
                    _convertService.Convert<IAccount, AccountContract>(previousSnapshot);
            }

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