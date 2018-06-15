namespace MarginTrading.AccountsManagement.InternalModels
{
    public enum AccountBalanceChangeReasonType
    {
        Deposit = 1,
        Withdraw = 2,
        PositionClosed = 3,
        Reset = 4,
        Swap = 5,
        Manual = 6,
        UnrealizedDailyPnL = 7,
        RealizedDailyPnL = 8,
        Commission = 9,
        Dividend = 10,
        OnBehalf = 11,
        Tax = 12,
    }
}