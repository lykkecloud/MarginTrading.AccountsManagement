namespace MarginTrading.AccountsManagement.InternalModels
{
    /// <summary>
    /// Why the account balance changed
    /// </summary>
    public enum AccountBalanceHistoryType
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
        /// Order was closed and the pnl was fixed in the balance
        /// </summary>
        OrderClosed = 3,

        /// <summary>
        /// An account balance was reset (used for demo accounts)
        /// </summary>
        Reset = 4,

        /// <summary>
        /// Swaps were applied 
        /// </summary>
        Swap = 5,

        /// <summary>
        /// Chnage was done manually via api (usually via BO)
        /// </summary>
        Manual = 6,
    }
}