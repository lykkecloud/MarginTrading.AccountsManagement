using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.Deposit.Commands;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;

namespace MarginTrading.AccountsManagement.Workflow.Deposit
{
    public class DepositSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IConvertService _convertService;
        private const string UpdateBalanceSource = "Deposit";

        public DepositSaga(IConvertService convertService, CqrsContextNamesSettings contextNames)
        {
            _convertService = convertService;
            _contextNames = contextNames;
        }

        /// <summary>
        /// The deposit has started => freeze the amount to be deposited.
        /// </summary>
        [UsedImplicitly]
        private Task Handle(DepositStartedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(_convertService.Convert<FreezeAmountForDepositInternalCommand>(evt),
                _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The amount was frozen the in the margin => update the balance
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForDepositFrozenEvent evt, ICommandSender sender)
        {
            sender.SendCommand(new BeginUpdateBalanceInternalCommand(evt.ClientId, evt.AccountId, evt.Amount,
                evt.OperationId, evt.Reason, UpdateBalanceSource), _contextNames.AccountsManagement);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Failed to freeze the amount in the margin => fail the withdrawal
        /// </summary>
        [UsedImplicitly]
        private Task Handle(AmountForDepositFreezeFailedEvent evt, ICommandSender sender)
        {
            sender.SendCommand(new FailDepositInternalCommand(evt.ClientId, evt.AccountId, evt.Amount,
                    evt.OperationId, "Failed to freeze amount for deposit: " + evt.Reason),
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
                sender.SendCommand(_convertService.Convert<CompleteDepositInternalCommand>(evt),
                    _contextNames.AccountsManagement);
            }

            return Task.CompletedTask;
        }
    }
}