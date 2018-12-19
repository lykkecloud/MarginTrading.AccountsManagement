using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.TemporaryCapital.Events;

namespace MarginTrading.AccountsManagement.Workflow.TemporaryCapital
{
    internal class TemporaryCapitalSaga
    {
        internal const string OperationName = "Deposit";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public TemporaryCapitalSaga(
            CqrsContextNamesSettings contextNames,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
        }

        [UsedImplicitly]
        public async Task Handle(GiveTemporaryCapitalStartedInternalEvent e, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<GiveTemporaryCapitalData>(
                OperationName,
                e.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(GiveTemporaryCapitalState.Initiated, GiveTemporaryCapitalState.Started))
            {
                
            }
        }
    }
}