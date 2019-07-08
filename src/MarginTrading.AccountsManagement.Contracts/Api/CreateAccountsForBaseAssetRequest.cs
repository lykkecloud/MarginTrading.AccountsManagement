// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountsForBaseAssetRequest
    {
        [NotNull]
        public string TradingConditionId { get; set; }
        [NotNull]
        public string BaseAssetId { get; set; }
    }
}