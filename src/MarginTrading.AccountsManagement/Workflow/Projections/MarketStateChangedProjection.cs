// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.Backend.Contracts.TradingSchedule;

namespace MarginTrading.AccountsManagement.Workflow.Projections
{
    public class MarketStateChangedProjection
    {
        private readonly IEodTaxFileMissingRepository _taxFileMissingRepository;
        private readonly ILog _log;
        
        private const string PlatformScheduleMarketId = nameof(PlatformScheduleMarketId);

        public MarketStateChangedProjection(ILog log, IEodTaxFileMissingRepository taxFileMissingRepository)
        {
            _log = log;
            _taxFileMissingRepository = taxFileMissingRepository;
        }
        
        [UsedImplicitly]
        public async Task Handle(MarketStateChangedEvent e)
        {
            await _log.WriteInfoAsync(
                nameof(MarketStateChangedProjection), 
                nameof(Handle), 
                e.ToJson(),
                $"Handling new MarketStateChanged event.");

            if (e.Id == PlatformScheduleMarketId && e.IsEnabled)
            {
                var tradingDay = e.EventTimestamp.Date;
                    
                await _taxFileMissingRepository.AddAsync(tradingDay);
                
                await _log.WriteInfoAsync(
                    nameof(MarketStateChangedProjection),
                    nameof(Handle),
                    new {tradingDay}.ToJson(),
                    "Added new tax file missing day");
            }
        }
    }
}