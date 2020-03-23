// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAccountBalanceCache
    {
        Task<IReadOnlyList<IAccountBalanceChange>> GetByStartDateAsync(string accountId, DateTime @from);
    }
}