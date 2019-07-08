// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateDefaultAccountsRequest
    {
        [NotNull]
        public string TradingConditionId { get; set; }

        [NotNull]
        public string ClientId { get; set; }
    }
}