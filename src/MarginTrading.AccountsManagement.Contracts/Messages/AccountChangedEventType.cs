namespace MarginTrading.AccountsManagement.Contracts.Messages
{
    /// <summary>
    /// What happend to the account
    /// </summary>
    public enum AccountChangedEventType
    {
        Created = 1,
        Updated = 2,
        Disabled = 3
    }
}