using System;
using Lykke.AzureStorage.Tables;
using MarginTrading.AccountsManagement.AccountHistoryBroker.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker.Repositories.SqlRepositories
{
    internal class AccountHistoryEntity : IAccountHistory
    {
        public string Id { get; set; }
        public string AccountId { get; set; }

        public DateTime ChangeTimestamp { get; set; }
        public string ClientId { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal Balance { get; set; }
        public decimal WithdrawTransferLimit { get; set; }
        public string Comment { get; set; }
        AccountBalanceChangeReasonType IAccountHistory.ReasonType => Enum.Parse<AccountBalanceChangeReasonType>(ReasonType);
        public string ReasonType { get; set; }
        public string EventSourceId { get; set; }
        public string LegalEntity { get; set; }
        public string AuditLog { get; set; }
    }
}