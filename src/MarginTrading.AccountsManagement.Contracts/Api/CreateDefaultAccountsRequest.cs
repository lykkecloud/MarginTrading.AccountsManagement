using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class CreateDefaultAccountsRequest
    {
        [NotNull]
        public string TradingConditionId { get; set; }
    }
}