// Copyright (c) 2019 Lykke Corp.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    [UsedImplicitly]
    [MessagePackObject]
    public class AccountContract
    {
        [Key(0)]
        public string Id { get; }

        [Key(1)]
        public string ClientId { get; }

        [Key(2)]
        public string TradingConditionId { get; }

        [Key(3)]
        public string BaseAssetId { get; }

        [Key(4)]
        public decimal Balance { get; }

        [Key(5)]
        public decimal WithdrawTransferLimit { get; }

        [Key(6)]
        public string LegalEntity { get; }

        [Key(7)]
        public bool IsDisabled { get; }

        [Key(8)]
        public DateTime ModificationTimestamp { get; }
        
        [Key(9)]
        public bool IsWithdrawalDisabled { get; }
        
        [Key(10)]
        public bool IsDeleted { get; }

        public AccountContract([NotNull] string id, [NotNull] string clientId, [NotNull] string tradingConditionId, 
            [NotNull] string baseAssetId, decimal balance, decimal withdrawTransferLimit, [NotNull] string legalEntity, 
            bool isDisabled, DateTime modificationTimestamp, bool isWithdrawalDisabled, bool isDeleted)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            TradingConditionId = tradingConditionId ?? throw new ArgumentNullException(nameof(tradingConditionId));
            BaseAssetId = baseAssetId ?? throw new ArgumentNullException(nameof(baseAssetId));
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            LegalEntity = legalEntity ?? throw new ArgumentNullException(nameof(legalEntity));
            IsDisabled = isDisabled;
            ModificationTimestamp = modificationTimestamp;
            IsWithdrawalDisabled = isWithdrawalDisabled;
            IsDeleted = isDeleted;
        }
    }
}