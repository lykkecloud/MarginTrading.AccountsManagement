using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.AccountsManagement.Contracts.Messages;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    [UsedImplicitly]
    public class EventSender : IEventSender
    {
        private readonly IConvertService _convertService;
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;

        private readonly IMessageProducer<AccountChangedEvent> _accountChangeMessageProducer;
        private readonly IMessageProducer<AccountHistoryContract> _accountHistoryMessageProducer;

        public EventSender(
            IRabbitMqService rabbitMqService,
            IReloadingManager<AccountManagementSettings> settings,
            IConvertService convertService,
            ILog log,
            ISystemClock systemClock)
        {
            _convertService = convertService;
            _log = log;
            _systemClock = systemClock;

            _accountChangeMessageProducer =
                rabbitMqService.GetProducer(settings.Nested(s => s.RabbitMq.AccountChangedExchange), true,
                    rabbitMqService.GetJsonSerializer<AccountChangedEvent>());

            _accountHistoryMessageProducer =
                rabbitMqService.GetProducer(settings.Nested(s => s.RabbitMq.AccountHistoryExchange), true,
                    rabbitMqService.GetJsonSerializer<AccountHistoryContract>());


        }
        
        public Task SendAccountUpdatedEvent(Account account)
        {
            return SendAccountChangedEvent(account, AccountChangedEventType.Updated);
        }

        public Task SendAccountCreatedEvent(Account account)
        {
            return SendAccountChangedEvent(account, AccountChangedEventType.Created);
        }

        public async Task SendAccountHistoryEvent(AccountHistoryContract model)
        {
            try
            {
                await _accountHistoryMessageProducer.ProduceAsync(model);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(EventSender), model.ToJson(), ex);
            }
        }

        private async Task SendAccountChangedEvent(Account account, AccountChangedEventType eventType)
        {
            var message = new AccountChangedEvent
            {
                Date = _systemClock.UtcNow.UtcDateTime,
                Account = _convertService.Convert<Account, AccountContract>(account),
                EventType = eventType,
            };

            try
            {
                await _accountChangeMessageProducer.ProduceAsync(message);
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(EventSender), message.ToJson(), ex);
            }
        }
    }
}