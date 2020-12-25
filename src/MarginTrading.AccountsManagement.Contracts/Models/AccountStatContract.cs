// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Account statistics for the current trading day
    /// </summary>
    [MessagePackObject]
    public class AccountStatContract
    {
        /// <summary>
        /// Account id
        /// </summary>
        [NotNull]
        [Key(0)]
        public string AccountId { get; }
        
        /// <summary>
        /// Creation timestamp
        /// </summary>
        [Key(1)]
        public DateTime Created { get; }
        
        /// <summary>
        /// Realised pnl for the day
        /// </summary>
        [Key(2)]
        public decimal RealisedPnl { get; }
        
        /// <summary>
        /// Deposit amount for the day
        /// </summary>
        [Key(3)]
        public decimal DepositAmount { get; }
        
        /// <summary>
        /// Withdrawal amount for the day
        /// </summary>
        [Key(4)]
        public decimal WithdrawalAmount { get; }
        
        /// <summary>
        /// Commission amount for the day
        /// </summary>
        [Key(5)]
        public decimal CommissionAmount { get; }
        
        /// <summary>
        /// Other account balance changed for the day
        /// </summary>
        [Key(6)]
        public decimal OtherAmount { get; }
        
        /// <summary>
        /// Current account balance
        /// </summary>
        [Key(7)]
        public decimal AccountBalance { get; }
        
        /// <summary>
        /// Account balance at the moment of previous EOD
        /// </summary>
        [Key(8)]
        public decimal PrevEodAccountBalance { get; }
        
        /// <summary>
        /// The available balance for account
        /// </summary>
        [Key(9)]
        public decimal DisposableCapital { get; }
        
        /// <summary>
        /// UnRealised pnl for the day
        /// </summary>
        [Key(10)]
        public decimal UnRealisedPnl { get; }

        /// <summary>
        /// Account name
        /// </summary>
        [Key(11)]
        public string AccountName { get; }
        
        /// <summary>
        /// Detailed account capital info
        /// </summary>
        [Key(12)]
        public AccountCapitalContract AccountCapitalDetails { get; }

        /// <summary>Balance + UnrealizedPnL</summary>
        [Key(13)]
        public decimal TotalCapital { get; }

        /// <summary>
        /// Margin used for maintenance of positions (considering MCO rule)
        /// = Max (CurrentlyUsedMargin, InitiallyUsedMargin/2)
        /// </summary>
        [Key(14)]
        public decimal UsedMargin { get; }

        /// <summary>
        /// UsedMargin / TotalCapital * 100
        /// </summary>
        [Key(15)]
        public decimal UsedMarginPercent { get; }

        /// <summary>TotalCapital - UsedMargin</summary>
        [Key(16)]
        public decimal FreeCapital { get; }

        /// <summary>Unrealized PnL</summary>
        [Key(17)]
        public decimal UnrealizedPnl { get; }

        /// <summary>Sum of all cash movements except for unrealized PnL</summary>
        [Key(18)]
        public decimal Balance { get; }

        /// <summary>Unrealized daily PnL</summary>
        [Key(19)]
        public decimal UnrealizedPnlDay { get; }

        /// <summary>Margin used by open positions</summary>
        [Key(20)]
        public decimal CurrentlyUsedMargin { get; }

        /// <summary>Margin used for initial open of existing positions</summary>
        [Key(21)]
        public decimal InitiallyUsedMargin { get; }

        /// <summary>Number of opened positions</summary>
        [Key(22)]
        public int OpenPositionsCount { get; }

        public AccountStatContract([NotNull] string accountId, DateTime created, decimal realisedPnl,
            decimal depositAmount, decimal withdrawalAmount, decimal commissionAmount, decimal otherAmount,
            decimal accountBalance, decimal prevEodAccountBalance, decimal disposableCapital,
            decimal unRealisedPnl, string accountName, AccountCapitalContract accountCapitalDetails,
            decimal totalCapital, decimal usedMargin, decimal usedMarginPercent, decimal freeCapital,
            decimal unrealizedPnl, decimal balance, decimal unrealizedPnlDay, decimal currentlyUsedMargin,
            decimal initiallyUsedMargin, int openPositionsCount)
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
        }
    }
}