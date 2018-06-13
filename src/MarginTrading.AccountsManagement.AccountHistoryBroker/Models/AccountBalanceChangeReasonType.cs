namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Models
{
    public enum AccountBalanceChangeReasonType
    {
        Deposit = 1,
        Withdraw = 2,
        PositionClosed = 3,
        Reset = 4,
        Swap = 5,
        Manual = 6,
    }
}