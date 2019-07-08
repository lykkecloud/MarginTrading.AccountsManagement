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

        public AccountStat([NotNull] string accountId, DateTime created, decimal realisedPnl, decimal depositAmount,
            decimal withdrawalAmount, decimal commissionAmount, decimal otherAmount, decimal accountBalance,
            decimal prevEodAccountBalance)
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
        }
    }
}