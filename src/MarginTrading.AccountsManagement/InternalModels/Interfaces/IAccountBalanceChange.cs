using System;

namespace MarginTrading.AccountsManagement.InternalModels.Interfaces
{
    public interface IAccountBalanceChange
    {
        string Id { get; }

        DateTime ChangeTimestamp { get; }

        string AccountId { get; }

        string ClientId { get; }

        decimal ChangeAmount { get; }

        decimal Balance { get; }

        decimal WithdrawTransferLimit { get; }

        string Comment { get; }

        AccountBalanceChangeReasonType ReasonType { get; }

        string EventSourceId { get; }

        string LegalEntity { get; }

        string AuditLog { get; }
        
        string Instrument { get; }
        
        DateTime TradingDate { get; }
    }
}