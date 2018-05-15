using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IAccountBalanceHistoryRepository
    {
        Task<List<AccountBalanceHistory>> GetAsync(string[] accountIds, DateTime? from, DateTime? to);
        Task AddAsync(AccountBalanceHistory history);
    }
}