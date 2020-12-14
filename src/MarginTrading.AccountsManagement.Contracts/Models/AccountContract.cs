// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
        public string Id { get; set; }

        [Key(1)]
        public string ClientId { get; set; }

        [Key(2)]
        public string TradingConditionId { get; set; }

        [Key(3)]
        public string BaseAssetId { get; set; }

        [Key(4)]
        public decimal Balance { get; set; }

        [Key(5)]
        public decimal WithdrawTransferLimit { get; set; }

        [Key(6)]
        public string LegalEntity { get; set; }

        [Key(7)]
        public bool IsDisabled { get; set; }

        [Key(8)]
        public DateTime ModificationTimestamp { get; set; }
        
        [Key(9)]
        public bool IsWithdrawalDisabled { get; set; }
        
        [Key(10)]
        public bool IsDeleted { get; set; }

        [Key(11)]
        public string AccountName { get; set; }

        public AccountContract([NotNull] string id, [NotNull] string clientId, [NotNull] string tradingConditionId, 
            [NotNull] string baseAssetId, decimal balance, decimal withdrawTransferLimit, [NotNull] string legalEntity, 
            bool isDisabled, DateTime modificationTimestamp, bool isWithdrawalDisabled, bool isDeleted, string accountName)
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
            AccountName = accountName;
        }
    }
}