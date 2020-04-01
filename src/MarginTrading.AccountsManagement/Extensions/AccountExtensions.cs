// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Extensions
{
    public static class AccountExtensions
    {
        public static decimal GetTemporaryCapital(this IAccount account)
        {
            return account?.TemporaryCapital?.Sum(x => x.Amount) ?? default;
        }
    }
}