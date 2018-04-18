namespace MarginTrading.AccountsManagement.Contracts.Messages
{
    public enum AccountHistoryTypeContract
    {
        Deposit = 1,
        Withdraw = 2,
        OrderClosed = 3,
        Reset = 4,
        Swap = 5,
        Manual = 6,
    }
}