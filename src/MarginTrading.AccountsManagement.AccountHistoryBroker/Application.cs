// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.SlackNotifications;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Extensions;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Services;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    internal class Application : BrokerApplicationBase<AccountChangedEvent>
    {
        private readonly IAccountHistoryRepository _accountHistoryRepository;
        private readonly Settings _settings;
        private readonly ILog _log;

        public Application(
            IAccountHistoryRepository accountHistoryRepository, 
            ILog log,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender)
            : base(log, slackNotificationsSender, applicationInfo, MessageFormat.MessagePack)
        {
            _accountHistoryRepository = accountHistoryRepository;
            _log = log;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.AccountHistory.ExchangeName;
        protected override string RoutingKey => nameof(AccountChangedEvent);

        protected override async Task HandleMessage(AccountChangedEvent accountChangedEvent)
        {
            try
            {
                if (accountChangedEvent.BalanceChange == null)
                {
                    await _log.WriteInfoAsync(nameof(HandleMessage), 
                        "No history event with BalanceChange=null is permitted to be written",
                        accountChangedEvent.ToJson());
                    return;
                }
                
                var accountHistory = Map(accountChangedEvent.BalanceChange);

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

        private static AccountHistory Map(AccountBalanceChangeContract accountBalanceChangeContract)
        {
            return new AccountHistory(
                id: accountBalanceChangeContract.Id,
                changeAmount: accountBalanceChangeContract.ChangeAmount,
                accountId: accountBalanceChangeContract.AccountId,
                changeTimestamp: accountBalanceChangeContract.ChangeTimestamp,
                clientId: accountBalanceChangeContract.ClientId,
                balance: accountBalanceChangeContract.Balance,
                withdrawTransferLimit: accountBalanceChangeContract.WithdrawTransferLimit,
                comment: accountBalanceChangeContract.Comment,
                reasonType: accountBalanceChangeContract.ReasonType.ToType<AccountBalanceChangeReasonType>(),
                eventSourceId: accountBalanceChangeContract.EventSourceId,
                legalEntity: accountBalanceChangeContract.LegalEntity,
                auditLog: accountBalanceChangeContract.AuditLog,
                instrument: accountBalanceChangeContract.Instrument,
                tradingDate: accountBalanceChangeContract.TradingDate);
        }
    }
}