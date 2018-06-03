using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class ChangeAccountRequest
    {
        [CanBeNull] 
        public string TradingConditionId { get; set; }
        public bool? IsDisabled { get; set; }
    }
}