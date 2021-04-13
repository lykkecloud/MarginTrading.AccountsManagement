// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Repositories
{
    public interface IEodTaxFileMissingRepository
    {
        void Initialize();
        Task AddAsync(DateTime tradingDay);
        Task RemoveAsync(DateTime tradingDay);
        Task<IEnumerable<DateTime>> GetAllDaysAsync();
    }
}