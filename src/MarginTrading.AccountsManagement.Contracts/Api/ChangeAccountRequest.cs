namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class ChangeAccountRequest
    {
        public string TradingConditionId { get; set; }
        public bool? IsDisabled { get; set; }
    }
}