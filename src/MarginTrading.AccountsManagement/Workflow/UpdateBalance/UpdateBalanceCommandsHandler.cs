using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Infrastructure;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using MarginTrading.AccountsManagement.Repositories;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;
using MarginTrading.AccountsManagement.Workflow.Withdrawal;
using Microsoft.Extensions.Internal;

namespace MarginTrading.AccountsManagement.Workflow.UpdateBalance
{
    internal class UpdateBalanceCommandsHandler
    {
        private readonly IAccountsRepository _accountsRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ISystemClock _systemClock;
        private readonly IConvertService _convertService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;

        public UpdateBalanceCommandsHandler(IOperationExecutionInfoRepository executionInfoRepository,
            IAccountsRepository accountsRepository,
            IChaosKitty chaosKitty, 
            ISystemClock systemClock,
            IConvertService convertService)
        {
            _accountsRepository = accountsRepository;
            _chaosKitty = chaosKitty;
            _systemClock = systemClock;
            _convertService = convertService;
            _executionInfoRepository = executionInfoRepository;
        }

        /// <summary>
        /// Handles internal command to change the balance
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(UpdateBalanceInternalCommand command,
            IEventPublisher publisher)
        {
            var executionInfo = _executionInfoRepository.GetOrAddAsync(
                operationName: Enum.GetName(typeof(AccountBalanceChangeReasonType), command.ChangeReasonType),
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<WithdrawalSaga.DepositData>(
                    operationName: Enum.GetName(typeof(AccountBalanceChangeReasonType), command.ChangeReasonType),
                    id: command.OperationId,
                    data: new WithdrawalSaga.DepositData
                    {
                        ClientId = command.ClientId,
                        AccountId = command.AccountId,
                        Amount = command.AmountDelta,
                        AuditLog = command.AuditLog,
                        State = WithdrawalSaga.State.UpdatingBalance,
                        Comment = command.Comment
                    }));

            IAccount account = null;
            try
            {
                account = await _accountsRepository.UpdateBalanceAsync(
                    operationId: command.OperationId,
                    clientId: command.ClientId,
                    accountId: command.AccountId,
                    amountDelta: command.AmountDelta,
                    changeLimit: false);
            }
            catch (Exception ex)
            {
                publisher.PublishEvent(new AccountBalanceChangeFailedEvent(command.OperationId, 
                    _systemClock.UtcNow.UtcDateTime, ex.Message, command.Source));
                return CommandHandlingResult.Ok(); //means no retries required
            }

            _chaosKitty.Meow(command.OperationId);

            var change = new AccountBalanceChangeContract(
                id: command.OperationId,
                changeTimestamp: account.ModificationTimestamp,
                accountId: account.Id,
                clientId: account.ClientId,
                changeAmount: command.AmountDelta,
                balance: account.Balance,
                withdrawTransferLimit: account.WithdrawTransferLimit,
                comment: command.Comment,
                reasonType: Convert(command.ChangeReasonType),
                eventSourceId: command.EventSourceId,
                legalEntity: account.LegalEntity,
                auditLog: command.AuditLog,
                instrument: command.AssetPairId,
                tradingDate: command.TradingDay);//TODO pass from API call

            var convertedAccount = Convert(account);

            publisher.PublishEvent(new AccountChangedEvent(change.ChangeTimestamp, command.Source, convertedAccount,
                AccountChangedEventTypeContract.BalanceUpdated, change));
            
            return CommandHandlingResult.Ok();
        }

        /// <summary>
        /// Handles external balance changing command
        /// </summary>
        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ChangeBalanceCommand command, IEventPublisher publisher)
        {
            return await Handle(new UpdateBalanceInternalCommand(
                operationId: command.OperationId,
                clientId: command.ClientId,
                accountId: command.AccountId,
                amountDelta: command.Amount,
                comment: command.Reason,
                auditLog: command.AuditLog,
                source: $"{command.ReasonType.ToString()} command",
                changeReasonType: command.ReasonType.ToType<AccountBalanceChangeReasonType>(),
                eventSourceId: command.EventSourceId,
                assetPairId: string.Empty,
                tradingDay: DateTime.UtcNow
            ), publisher);
        }

        private AccountContract Convert(IAccount account)
        {
            return _convertService.Convert<AccountContract>(account);
        }

        private AccountBalanceChangeReasonTypeContract Convert(AccountBalanceChangeReasonType reasonType)
        {
            return _convertService.Convert<AccountBalanceChangeReasonTypeContract>(reasonType);
        }
    }
}