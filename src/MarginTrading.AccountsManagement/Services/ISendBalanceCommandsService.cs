using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface ISendBalanceCommandsService
    {
        Task ChargeManuallyAsync(string clientId, string accountId, decimal amountDelta, string operationId,
            string reason, string source);

        Task WithdrawAsync(string clientId, string accountId, decimal amountDelta,
            string operationId, string reason);

        Task DepositAsync(string clientId, string accountId, decimal amountDelta,
            string operationId, string reason);
    }
}