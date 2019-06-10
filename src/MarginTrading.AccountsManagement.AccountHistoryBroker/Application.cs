using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Services;
using MarginTrading.AccountsManagement.Contracts.Events;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    internal class Application : BrokerApplicationBase<AccountChangedEvent>
    {
        private readonly IAccountHistoryRepository _accountHistoryRepository;
        private readonly Settings _settings;
        private readonly IConvertService _convertService;
        private readonly ILog _log;

        public Application(
            IAccountHistoryRepository accountHistoryRepository, 
            ILog log,
            Settings settings, 
            CurrentApplicationInfo applicationInfo, 
            IConvertService convertService, 
            ISlackNotificationsSender slackNotificationsSender)
            : base(log, slackNotificationsSender, applicationInfo, MessageFormat.MessagePack)
        {
            _accountHistoryRepository = accountHistoryRepository;
            _log = log;
            _settings = settings;
            _convertService = convertService;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.AccountHistory.ExchangeName;
        protected override string RoutingKey => nameof(AccountChangedEvent);

        protected override async Task HandleMessage(AccountChangedEvent accountChangedEvent)
        {
            try
            {
                var accountHistory = _convertService.Convert<AccountHistory>(accountChangedEvent.BalanceChange);

                if (accountHistory.ChangeAmount != 0)
                {
                    await _accountHistoryRepository.InsertAsync(accountHistory);
                }
            }
            catch (Exception exception)
            {
                await _log.WriteErrorAsync(nameof(AccountHistoryBroker), nameof(HandleMessage), exception);
                throw;
            }
        }
    }
}