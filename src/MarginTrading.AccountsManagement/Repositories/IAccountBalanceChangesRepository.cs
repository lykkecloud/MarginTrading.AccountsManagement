using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAccountBalanceChangesRepository
    {
        Task<List<AccountBalanceChange>> GetAsync(string[] accountIds, DateTime? from, DateTime? to);
        Task AddAsync(AccountBalanceChange change);
    }
}