using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;
using Refit;

namespace MarginTrading.AccountsManagement.Contracts
{
    /// <summary>
    /// Provide access to account balance history
    /// </summary>
    public interface IAccountBalanceHistoryApi
    {
        /// <summary>
        /// Get account balance change history by account Id, and optionally by dates
        /// </summary>
        [Get("/api/balance-history/by-account/{accountId}")]
        Task<Dictionary<string, AccountBalanceChangeContract[]>> ByAccount([NotNull] string accountId,
            [CanBeNull][Query] DateTime? @from = null, [CanBeNull][Query] DateTime? to = null);
    }
}