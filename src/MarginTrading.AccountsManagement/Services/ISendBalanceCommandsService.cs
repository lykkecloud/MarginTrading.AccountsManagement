using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ISendBalanceCommandsService
    {
        Task<string> ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta, string operationId,
            string reason, string source, string auditLog);

        Task<string> WithdrawAsync(string clientId, string accountId, decimal amountDelta,
            string operationId, string reason, string auditLog);

        Task<string> DepositAsync(string clientId, string accountId, decimal amountDelta,
            string operationId, string reason, string auditLog);
    }
}