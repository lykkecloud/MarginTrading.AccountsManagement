using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAccountBalanceChangesRepository
    {
        Task<PaginatedResponse<IAccountBalanceChange>> GetByPagesAsync(string accountId,
            DateTime? @from = null, DateTime? to = null, AccountBalanceChangeReasonType? reasonType = null, 
            string assetPairId = null, int? skip = null, int? take = null, bool isAscendingOrder = true);

        Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId,
            DateTime? @from = null, DateTime? to = null, AccountBalanceChangeReasonType? reasonType = null);
        
        Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, string eventSourceId);

        Task<decimal> GetRealizedDailyPnl(string accountId);
        
        Task AddAsync(IAccountBalanceChange change);
        
        Task<decimal> GetBalanceAsync(string accountId, DateTime date);
    }
}