using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class SendBalanceCommandsService : ISendBalanceCommandsService
    {
        public ICqrsEngine _cqrsEngine { get; set; }//property injection.. to workaround circular dependency
        private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;

        public SendBalanceCommandsService(CqrsContextNamesSettings cqrsContextNamesSettings)
        {
            _cqrsContextNamesSettings = cqrsContextNamesSettings;
        }

        public Task<string> ChargeManuallyAsync(string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string source, string auditLog,
            AccountBalanceChangeReasonType type, string eventSourceId, string assetPairId, DateTime tradingDay)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new UpdateBalanceInternalCommand(
                    operationId: operationId,
                    accountId: accountId,
                    amountDelta: amountDelta,
                    comment: reason,
                    auditLog: auditLog,
                    source: source,
                    changeReasonType: type,
                    eventSourceId: eventSourceId,
                    assetPairId: assetPairId,
                    tradingDay: tradingDay),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> WithdrawAsync(string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string auditLog)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new WithdrawCommand(
                    operationId: operationId,
                    clientId: null,
                    accountId: accountId,
                    amount: amountDelta,
                    comment: reason,
                    auditLog: auditLog),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> DepositAsync(string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string auditLog)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new DepositCommand(
                    operationId: operationId,
                    clientId: null,
                    accountId: accountId,
                    amount: amountDelta,
                    comment: reason,
                    auditLog: auditLog),
                _cqrsContextNamesSettings.AccountsManagement,
                _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }
    }
}