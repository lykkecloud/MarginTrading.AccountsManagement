namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountRequest
    {
        public string BaseAssetId { get; set; }
        public string TradingConditionId{ get; set; }
    }
}