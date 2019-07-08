// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IAccuracyService
    {
        Task<decimal> ToAccountAccuracy(decimal amount, string accountBaseAsset, string operationName);
    }
}