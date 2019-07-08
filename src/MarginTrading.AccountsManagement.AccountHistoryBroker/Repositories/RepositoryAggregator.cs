// Copyright (c) 2019 Lykke Corp.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories
{
    internal class RepositoryAggregator : IAccountHistoryRepository
    {
        private readonly List<IAccountHistoryRepository> _repositories;

        public RepositoryAggregator(IEnumerable<IAccountHistoryRepository> repositories)
        {
            _repositories = new List<IAccountHistoryRepository>();
            _repositories.AddRange(repositories);
        }

        public async Task InsertAsync(IAccountHistory report)
        {
            foreach (var item in _repositories)
            {
                await item.InsertAsync(report);
            }
        }
    }
}
