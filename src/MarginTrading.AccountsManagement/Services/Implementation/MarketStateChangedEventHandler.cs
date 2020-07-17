// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.Backend.Contracts.TradingSchedule;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class MarketStateChangedEventHandler : IHostedService
    {
        private readonly ILog _log;
        private readonly RabbitMqSubscriber<MarketStateChangedEvent> _subscriber;
        private readonly IEodTaxFileMissingRepository _taxFileMissingRepository;

        private const string PlatformScheduleMarketId = nameof(PlatformScheduleMarketId);

        public MarketStateChangedEventHandler(
            RabbitMqSubscriber<MarketStateChangedEvent> subscriber, 
            IEodTaxFileMissingRepository taxFileMissingRepository,
            ILog log)
        {
            _subscriber = subscriber;
            _taxFileMissingRepository = taxFileMissingRepository;
            _log = log;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscriber
                .Subscribe(Handle)
                .Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriber.Stop();

            return Task.CompletedTask;
        }
        
        private async Task Handle(MarketStateChangedEvent @event)
        {
            await _log.WriteInfoAsync(
                nameof(MarketStateChangedEventHandler), 
                nameof(Handle), 
                @event.ToJson(),
                $"Handling new MarketStateChanged event.");

            if (@event.Id == PlatformScheduleMarketId && @event.IsEnabled)
            {
                var tradingDay = @event.EventTimestamp.Date;
                    
                await _taxFileMissingRepository.AddAsync(tradingDay);
                
                await _log.WriteInfoAsync(
                    nameof(MarketStateChangedEventHandler),
                    nameof(Handle),
                    new {tradingDay}.ToJson(),
                    "Added new tax file missing day");
            }
        }
    }
}