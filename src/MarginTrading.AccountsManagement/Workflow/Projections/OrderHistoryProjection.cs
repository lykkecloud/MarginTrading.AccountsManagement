using System;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Services;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using Newtonsoft.Json;
using BrokerFeature = Lykke.Snow.Mdm.Contracts.BrokerFeatures.BrokerFeature;
using System.Linq;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Snow.Mdm.Contracts.BrokerFeatures;
using MarginTrading.AccountsManagement.Extensions.AdditionalInfo;
using MarginTrading.AccountsManagement.InternalModels;
using Microsoft.FeatureManagement;

namespace MarginTrading.AccountsManagement.Workflow.Projections
{
    public class OrderHistoryProjection
    {
        private readonly IComplexityWarningRepository _complexityWarningRepository;
        private readonly IAccountManagementService _accountManagementService;
        private readonly IFeatureManager _featureManager;
        private readonly AccountManagementSettings _settings;
        private readonly ILog _log;

        public OrderHistoryProjection(IComplexityWarningRepository complexityWarningRepository, 
            IAccountManagementService accountManagementService,
            IFeatureManager featureManager,
            AccountManagementSettings settings, 
            ILog log)
        {
            _complexityWarningRepository = complexityWarningRepository;
            _accountManagementService = accountManagementService;
            _featureManager = featureManager;
            _settings = settings;
            _log = log;
        }

        public async Task Handle(OrderHistoryEvent e)
        {
            if (!await _featureManager.IsEnabledAsync(BrokerFeature.ProductComplexityWarning))
            {
                return;
            }

            if (_settings.ComplexityWarningsCount <= 0) 
            {
                throw new InvalidOperationException($"Broker {_settings.BrokerId} feature {BrokerFeature.ProductComplexityWarning} is enabled, " +
                                                    $"but {nameof(_settings.ComplexityWarningsCount)} = {_settings.ComplexityWarningsCount} is not valid ");
            }

            var order = e.OrderSnapshot;
            var isBasicOrder = new[]
                {
                    OrderTypeContract.Market,
                    OrderTypeContract.Limit,
                    OrderTypeContract.Stop
                }
                .Contains(order.Type);

            if (!isBasicOrder || !order.ProductComplexityConfirmationReceived())
            {
                return;
            }

            await _log.WriteInfoAsync(nameof(OrderHistoryProjection), 
                nameof(Handle),
                $"Product complexity confirmation received for account {order.AccountId} and orderId {order.Id}");

            var entity = await _complexityWarningRepository.GetOrCreate(order.AccountId,
                () => ComplexityWarningState.Start(order.AccountId));

            entity.OnConfirmedOrderReceived(order.Id, order.CreatedTimestamp, _settings.ComplexityWarningsCount, out var confirmationFlagShouldBeSwitched);

            if (confirmationFlagShouldBeSwitched)
            {
                await _accountManagementService.UpdateComplexityWarningFlag(order.AccountId, shouldShowProductComplexityWarning: false);

                await _log.WriteInfoAsync(nameof(OrderHistoryProjection),
                    nameof(Handle), 
                    $"Flag {BrokerFeature.ProductComplexityWarning} for account {entity.AccountId} is switched to off");
            }

            await _complexityWarningRepository.Save(entity);
        }
    }
}
