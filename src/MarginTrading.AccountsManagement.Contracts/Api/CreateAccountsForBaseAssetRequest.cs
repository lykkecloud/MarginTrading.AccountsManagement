namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountsForBaseAssetRequest
    {
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
    }
}