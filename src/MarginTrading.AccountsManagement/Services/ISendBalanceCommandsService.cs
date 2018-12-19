using System;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ISendBalanceCommandsService
    {
        Task<string> ChargeManuallyAsync(string accountId, decimal amountDelta, string operationId,
            string reason, string source, string auditLog, AccountBalanceChangeReasonType type, string eventSourceId,
            string assetPairId, DateTime tradingDate);

        Task<string> WithdrawAsync(string accountId, decimal amountDelta,
            string operationId, string reason, string auditLog);

        Task<string> DepositAsync(string accountId, decimal amountDelta,
            string operationId, string reason, string auditLog);

        Task<string> GiveTemporaryCapital(string eventSourceId, string accountId, decimal amount,
            string reason, string auditLog);

        Task<string> RevokeTemporaryCapital(string eventSourceId, string accountId, string revokeEventSourceId,
            string auditLog);
    }
}