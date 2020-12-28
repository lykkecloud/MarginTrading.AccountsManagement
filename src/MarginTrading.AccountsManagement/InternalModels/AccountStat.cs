// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public class AccountStat
    {
        [NotNull] public string AccountId { get; }
        
        public DateTime Created { get; }
        
        public decimal RealisedPnl { get; }
        
        public decimal DepositAmount { get; }
        
        public decimal WithdrawalAmount { get; }
        
        public decimal CommissionAmount { get; }
        
        public decimal OtherAmount { get; }
        
        public decimal AccountBalance { get; }
        
        public decimal PrevEodAccountBalance { get; }
        
        public decimal DisposableCapital { get; }
        
        public decimal UnRealisedPnl { get; }

        public string AccountName { get; }
        
        public AccountCapital AccountCapitalDetails { get; }

        public decimal TotalCapital { get; }

        public decimal UsedMargin { get; }

        public decimal UsedMarginPercent { get; }

        public decimal FreeCapital { get; }

        public decimal UnrealizedPnl { get; }

        public decimal Balance { get; }

        public decimal UnrealizedPnlDay { get; }

        public decimal CurrentlyUsedMargin { get; }

        public decimal InitiallyUsedMargin { get; }

        public int OpenPositionsCount { get; }

        public DateTime LastBalanceChangeTime { get; }

        public AccountStat([NotNull] string accountId, DateTime created, decimal realisedPnl, decimal depositAmount,
            decimal withdrawalAmount, decimal commissionAmount, decimal otherAmount, decimal accountBalance,
            decimal prevEodAccountBalance, decimal disposableCapital, decimal unRealisedPnl, string accountName, 
            AccountCapital accountCapitalDetails, decimal totalCapital, decimal usedMargin, decimal usedMarginPercent, 
            decimal freeCapital, decimal unrealizedPnl, decimal balance, decimal unrealizedPnlDay, decimal currentlyUsedMargin, 
            decimal initiallyUsedMargin, int openPositionsCount, DateTime lastBalanceChangeTime)
        {
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Created = created;
            RealisedPnl = realisedPnl;
            DepositAmount = depositAmount;
            WithdrawalAmount = withdrawalAmount;
            CommissionAmount = commissionAmount;
            OtherAmount = otherAmount;
            AccountBalance = accountBalance;
            PrevEodAccountBalance = prevEodAccountBalance;
            DisposableCapital = disposableCapital;
            UnRealisedPnl = unRealisedPnl;
            AccountName = accountName;
            AccountCapitalDetails = accountCapitalDetails;
            TotalCapital = totalCapital;
            UsedMargin = usedMargin;
            UsedMarginPercent = usedMarginPercent;
            FreeCapital = freeCapital;
            UnrealizedPnl = unrealizedPnl;
            Balance = balance;
            UnrealizedPnlDay = unrealizedPnlDay;
            CurrentlyUsedMargin = currentlyUsedMargin;
            InitiallyUsedMargin = initiallyUsedMargin;
            OpenPositionsCount = openPositionsCount;
            LastBalanceChangeTime = lastBalanceChangeTime;
        }
    }
}