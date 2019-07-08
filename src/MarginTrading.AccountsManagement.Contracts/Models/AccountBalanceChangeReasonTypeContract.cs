// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    /// <summary>
    /// Why the account balance changed
    /// </summary>
    public enum AccountBalanceChangeReasonTypeContract
    {
        /// <summary>
        /// Funds were deposited
        /// </summary>
        Deposit = 1,

        /// <summary>
        /// Funds were withdrawn
        /// </summary>
        Withdraw = 2,

        /// <summary>
        /// An account balance was reset (used for demo accounts)
        /// </summary>
        Reset = 4,

        /// <summary>
        /// Swaps were applied 
        /// </summary>
        Swap = 5,

        /// <summary>
        /// Change was done manually via api (usually via BO) / Temporary cash adjustment / Compensation payments
        /// </summary>
        Manual = 6,
        
        /// <summary>
        /// Unrealized daily PnL
        /// </summary>
        UnrealizedDailyPnL = 7,
        
        /// <summary>
        /// Realized PnL
        /// </summary>
        RealizedPnL = 8,
        
        /// <summary>
        /// Commissions
        /// </summary>
        Commission = 9,
        
        /// <summary>
        /// Compensation payments for dividends
        /// </summary>
        Dividend = 10,
        
        /// <summary>
        /// On behalf fees
        /// </summary>
        OnBehalf = 11,
        
        /// <summary>
        /// Total tax
        /// </summary>
        Tax = 12,
        
        /// <summary>
        /// Temporary cash adjustment
        /// </summary>
        TemporaryCashAdjustment = 13,
        
        /// <summary>
        /// Compensation payments
        /// </summary>
        CompensationPayments = 14,
        
        /// <summary>
        /// New account was created
        /// </summary>
        Create = 15,
    }
}