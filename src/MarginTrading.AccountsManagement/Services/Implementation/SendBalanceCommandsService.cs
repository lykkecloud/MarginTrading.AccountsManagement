using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.Settings;
using MarginTrading.AccountsManagement.Workflow.UpdateBalance.Commands;

namespace MarginTrading.AccountsManagement.Services.Implementation
{
    public class SendBalanceCommandsService : ISendBalanceCommandsService
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;

        public SendBalanceCommandsService(ICqrsEngine cqrsEngine, CqrsContextNamesSettings cqrsContextNamesSettings)
        {
            _cqrsEngine = cqrsEngine;
            _cqrsContextNamesSettings = cqrsContextNamesSettings;
        }

        public Task<string> ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason, string source)
        {
            source.RequiredNotNullOrWhiteSpace(nameof(source));
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new BeginBalanceUpdateInternalCommand(clientId, accountId, amountDelta,
                    operationId, reason, source),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> WithdrawAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new BeginWithdrawalCommand(clientId, accountId, amountDelta, operationId,
                    reason),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }

        public Task<string> DepositAsync(string clientId, string accountId, decimal amountDelta,
            [CanBeNull] string operationId, string reason)
        {
            operationId = operationId ?? Guid.NewGuid().ToString();
            _cqrsEngine.SendCommand(
                new BeginDepositCommand(clientId, accountId, amountDelta, operationId,
                    reason),
                _cqrsContextNamesSettings.AccountsManagement, _cqrsContextNamesSettings.AccountsManagement);
            return Task.FromResult(operationId);
        }
    }
}