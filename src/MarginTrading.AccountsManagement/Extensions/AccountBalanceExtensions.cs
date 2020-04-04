// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Extensions
{
    public static class AccountBalanceExtensions
    {
        public static decimal GetTotalByType(this IEnumerable<IAccountBalanceChange> history, AccountBalanceChangeReasonType type)
        {
            if (history == null || !history.Any())
                return 0;

            return history
                .Where(x => x.ReasonType == type)
                .Sum(x => x.ChangeAmount);
        }
    }
}