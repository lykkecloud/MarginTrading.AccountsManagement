using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateAccountRequest
    {
        [NotNull]
        public string AccountId { get; set; }
        [NotNull]
        public string BaseAssetId { get; set; }
        [CanBeNull]
        public string TradingConditionId{ get; set; }
    }
}