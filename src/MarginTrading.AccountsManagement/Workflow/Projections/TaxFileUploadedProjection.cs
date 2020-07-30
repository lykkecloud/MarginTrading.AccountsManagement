// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using BookKeeper.Client.Workflow.Events;
using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Repositories;

namespace MarginTrading.AccountsManagement.Workflow.Projections
{
    public class TaxFileUploadedProjection
    {
        private readonly IEodTaxFileMissingRepository _taxFileMissingRepository;
        private readonly ILog _log;

        public TaxFileUploadedProjection(ILog log, IEodTaxFileMissingRepository taxFileMissingRepository)
        {
            _log = log;
            _taxFileMissingRepository = taxFileMissingRepository;
        }
        
        [UsedImplicitly]
        public async Task Handle(TaxFileUploadedEvent e)
        {
            await _log.WriteInfoAsync(
                nameof(TaxFileUploadedProjection), 
                nameof(Handle), 
                e.ToJson(),
                $"Handling new TaxFileUploadedEvent event.");

            await _taxFileMissingRepository.RemoveAsync(e.TradingDay);

            await _log.WriteInfoAsync(
                nameof(TaxFileUploadedProjection), 
                nameof(Handle), 
                new {e.TradingDay}.ToJson(),
                $"Tax file missing entity has been deleted.");
        }
    }
}