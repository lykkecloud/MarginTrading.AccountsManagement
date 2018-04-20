namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountRequest
    {
        public string AccountId { get; set; }
        public string BaseAssetId { get; set; }
        public string TradingConditionId{ get; set; }
    }
}