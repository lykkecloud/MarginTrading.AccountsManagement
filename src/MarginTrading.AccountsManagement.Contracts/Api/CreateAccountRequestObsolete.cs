// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountRequestObsolete
    {
        [NotNull]
        public string AccountId { get; set; }
        [NotNull]
        public string BaseAssetId { get; set; }
        [CanBeNull]
        public string TradingConditionId{ get; set; }
    }
}