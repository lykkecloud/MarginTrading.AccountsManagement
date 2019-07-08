// Copyright (c) 2019 Lykke Corp.

using System.Threading.Tasks;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories
{
    public interface IAccountHistoryRepository
    {
        Task InsertAsync(IAccountHistory entity);
    }
}