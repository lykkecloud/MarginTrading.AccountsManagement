using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Services
{
    /// <summary>
    /// Negative account balance protection (ESMA, NBP rule)
    /// </summary>
    public interface INegativeProtectionService
    {
        /// <summary>
        /// If a portfolio of a client is negative after closing all positions, then ChargeManually is called
        /// to credit the corresponding amount to the account to cover loss.
        /// Charging is called with a special type "CompensationPayments", event is logged in a common way.
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<decimal?> CheckAsync(string operationId, IAccount account);
    }
}