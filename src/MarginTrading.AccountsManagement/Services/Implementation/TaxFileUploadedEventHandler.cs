// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using BookKeeper.Client.Workflow.Events;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.AccountsManagement.Repositories;
using Microsoft.Extensions.Hosting;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class TaxFileUploadedEventHandler : IHostedService
    {
        private readonly ILog _log;
        private readonly RabbitMqSubscriber<TaxFileUploadedEvent> _subscriber;
        private readonly IEodTaxFileMissingRepository _taxFileMissingRepository;

        public TaxFileUploadedEventHandler(
            IEodTaxFileMissingRepository taxFileMissingRepository, 
            RabbitMqSubscriber<TaxFileUploadedEvent> subscriber, 
            ILog log)
        {
            _taxFileMissingRepository = taxFileMissingRepository;
            _subscriber = subscriber;
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
        
        private async Task Handle(TaxFileUploadedEvent @event)
        {
            await _log.WriteInfoAsync(
                nameof(TaxFileUploadedEventHandler), 
                nameof(Handle), 
                @event.ToJson(),
                $"Handling new TaxFileUploadedEvent event.");

            await _taxFileMissingRepository.RemoveAsync(@event.TradingDay);

            await _log.WriteInfoAsync(
                nameof(TaxFileUploadedEventHandler), 
                nameof(Handle), 
                new {@event.TradingDay}.ToJson(),
                $"Tax file missing entity has been deleted.");
        }
    }
}