using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    public class UpdateBalanceSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IConvertService _convertService;

        public UpdateBalanceSaga(CqrsContextNamesSettings contextNames, IConvertService convertService)
        {
            _contextNames = contextNames;
            _convertService = convertService;
        }

        /// <summary>
        /// The operation has started => proceed
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AccountBalanceUpdateStartedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(_convertService.Convert<UpdateBalanceInternalCommand>(evt),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }
    }
}