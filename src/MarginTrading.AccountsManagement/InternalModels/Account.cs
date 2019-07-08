// Copyright (c) 2019 Lykke Corp.

using System;
using System.Collections.Generic;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Extensions;
using MarginTrading.AccountsManagement.Infrastructure.Implementation;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class Account : IAccount
    {
        public Account([NotNull] string id, [NotNull] string clientId, [NotNull] string tradingConditionId, 
            [NotNull] string baseAssetId, decimal balance, decimal withdrawTransferLimit, [NotNull] string legalEntity, 
            bool isDisabled, bool isWithdrawalDisabled, bool isDeleted, DateTime modificationTimestamp)
        {
            Id = id.RequiredNotNullOrWhiteSpace(nameof(id));
            ClientId = clientId.RequiredNotNullOrWhiteSpace(nameof(clientId));
            TradingConditionId = tradingConditionId.RequiredNotNullOrWhiteSpace(nameof(tradingConditionId));
            BaseAssetId = baseAssetId.RequiredNotNullOrWhiteSpace(nameof(baseAssetId));
            Balance = balance;
            WithdrawTransferLimit = withdrawTransferLimit;
            LegalEntity = legalEntity.RequiredNotNullOrWhiteSpace(nameof(legalEntity));
            IsDisabled = isDisabled;
            IsWithdrawalDisabled = isWithdrawalDisabled;
            IsDeleted = isDeleted;
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
        
        public bool IsWithdrawalDisabled { get; }
        
        public bool IsDeleted { get; }

        public DateTime ModificationTimestamp { get; }
        
        
        public List<TemporaryCapital> TemporaryCapital { get; set; } = new List<TemporaryCapital>(); 
            

        public List<string> LastExecutedOperations { get; set; } = new List<string>();
    }
}