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
        /// Position was closed and the pnl was fixed in the balance
        /// </summary>
        PositionClosed = 3,

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
        /// Realized daily PnL
        /// </summary>
        RealizedDailyPnL = 8,
        
        /// <summary>
        /// Commissions
        /// </summary>
        Commission = 9,
        
        /// <summary>
        /// Compensations payments for dividends
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
    }
}