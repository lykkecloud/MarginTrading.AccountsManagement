namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class UpdateClientTradingConditionRequest
    {
        public string ClientId { get; set; }

        public string TradingConditionId { get; set; }
    }
}
