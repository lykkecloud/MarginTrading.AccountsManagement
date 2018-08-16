using System;
using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.InternalModels.Interfaces
{
    public interface IAccount
    {
        string Id { get; }
        string ClientId { get; }
        string TradingConditionId { get; }
        string BaseAssetId { get; }
        decimal Balance { get; }
        decimal WithdrawTransferLimit { get; }
        string LegalEntity { get; }
        bool IsDisabled { get; }
        bool IsWithdrawalDisabled { get; }
        DateTime ModificationTimestamp { get; }
        List<string> LastExecutedOperations { get; }
    }
}