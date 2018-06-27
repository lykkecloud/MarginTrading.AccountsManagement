using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAccountBalanceChangesRepository
    {
        Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string[] accountIds, DateTime? @from, DateTime? to);
        Task<IReadOnlyList<IAccountBalanceChange>> GetAsync(string accountId, string eventSourceId);
        Task AddAsync(IAccountBalanceChange change);
    }
}