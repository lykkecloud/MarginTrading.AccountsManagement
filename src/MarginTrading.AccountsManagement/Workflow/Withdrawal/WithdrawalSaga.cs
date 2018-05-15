using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.TradingEngineMock.Contracts;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal.Commands;

namespace MarginTrading.AccountsManagement.Workflow.Withdrawal
{
    internal class WithdrawalSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IConvertService _convertService;
        private const string UpdateBalanceSource = "Withdrawal";

        public WithdrawalSaga(IConvertService convertService, CqrsContextNamesSettings contextNames)
        {
            _convertService = convertService;
            _contextNames = contextNames;
        }

        /// <summary>
        /// The withdrawal has started => ask the backend to freeze the amount in the margin.
        /// </summary>
        [UsedImplicitly]
        private Task Handle(WithdrawalStartedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(_convertService.Convert<FreezeAmountForWithdrawalCommand>(evt),
                _contextNames.TradingEngine);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The backend frozen the amount in the margin => update the balance
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForWithdrawalFrozenEvent evt, ICommandSender sender)
        {
            sender.SendCommand(new BeginBalanceUpdateInternalCommand(evt.ClientId, evt.AccountId, -evt.Amount,
                    evt.OperationId, evt.Reason, UpdateBalanceSource),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The backend failed to freeze the amount in the margin => fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForWithdrawalFreezeFailedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(new FailWithdrawalInternalCommand(evt.ClientId, evt.AccountId, evt.Amount,
                    evt.OperationId, "Backend failed to freeze amount: " + evt.Reason),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The balance has changed => finish the operation
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AccountBalanceChangedEvent evt, ICommandSender sender)
        {
            if (evt.Source == UpdateBalanceSource)
            {
                sender.SendCommand(_convertService.Convert<CompleteWithdrawalInternalCommand>(evt),
                    _contextNames.AccountsManagement);
            }

            return Task.CompletedTask;
        }
    }
}