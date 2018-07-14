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
        public ICqrsEngine _cqrsEngine { get; set; }//property injection.. to workaround circular dependency
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IMessageProducer<NegativeProtectionEvent> _negativeProtectionMessageProducer;

        public EventSender(
            IRabbitMqService rabbitMqService,
            IConvertService convertService,
            ISystemClock systemClock,
            CqrsContextNamesSettings contextNames,
            IReloadingManager<RabbitConnectionSettings> negativeProtectionSettings)
        {
            _convertService = convertService;
            _systemClock = systemClock;
            _contextNames = contextNames;
            
            _negativeProtectionMessageProducer =
                rabbitMqService.GetProducer(negativeProtectionSettings, true,
                    rabbitMqService.GetMsgPackSerializer<NegativeProtectionEvent>());
        }

        public async Task SendNegativeProtectionMessage(string correlationId, string causationId, string clientId,
            string accountId, decimal amount)
        {
            await _negativeProtectionMessageProducer.ProduceAsync(new NegativeProtectionEvent(
                id: Guid.NewGuid().ToString("N"),
                correlationId: correlationId,
                causationId: causationId,
                eventTimestamp: _systemClock.UtcNow.UtcDateTime,
                clientId: clientId,
                accountId: accountId,
                amount: amount
            ));
        }

        public void SendAccountChangedEvent(string source, IAccount account, AccountChangedEventTypeContract eventType,
            AccountBalanceChangeContract balanceChangeContract = null)
        {
            _cqrsEngine.PublishEvent(
                new AccountChangedEvent(
                    account.ModificationTimestamp,
                    source,
                    _convertService.Convert<IAccount, AccountContract>(account),
                    eventType,
                    balanceChangeContract),
                _contextNames.AccountsManagement);
        }
    }
}