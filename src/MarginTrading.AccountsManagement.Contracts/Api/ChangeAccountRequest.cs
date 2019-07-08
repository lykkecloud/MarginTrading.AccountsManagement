// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    /// <summary>
    /// Account modification parameters. Only one parameter may be changed in a request.
    /// </summary>
    public class ChangeAccountRequest
    {
        [CanBeNull] 
        public string TradingConditionId { get; set; }
        public bool? IsDisabled { get; set; }
        public bool? IsWithdrawalDisabled { get; set; }
    }
}