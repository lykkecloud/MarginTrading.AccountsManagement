using System;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class Account
    {
        public Account(string id, string clientId, string tradingConditionId, string baseAssetId, decimal balance,
            decimal withdrawTransferLimit, string legalEntity, bool isDisabled, DateTimeOffset modificationTimestamp)
        {
            Id = id.RequiredNotNullOrWhiteSpace(nameof(id));
            ClientId = clientId.RequiredNotNullOrWhiteSpace(nameof(clientId));
            TradingConditionId = tradingConditionId.RequiredNotNullOrWhiteSpace(nameof(tradingConditionId));
            BaseAssetId = baseAssetId.RequiredNotNullOrWhiteSpace(nameof(baseAssetId));
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            LegalEntity = legalEntity.RequiredNotNullOrWhiteSpace(nameof(legalEntity));
            IsDisabled = isDisabled;
            ModificationTimestamp = modificationTimestamp;
        }

        public string Id { get; }
        public string ClientId { get; }
        public string TradingConditionId { get; }
        public string BaseAssetId { get; }
        public decimal Balance { get; }
        public decimal WithdrawTransferLimit { get; }
        public string LegalEntity { get; }
        public bool IsDisabled { get; }
        public DateTimeOffset ModificationTimestamp { get; }
    }
}