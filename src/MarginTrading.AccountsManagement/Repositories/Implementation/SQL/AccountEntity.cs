using System;
using System.Collections.Generic;
using Common;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;
using Newtonsoft.Json;

namespace MarginTrading.AccountsManagement.Repositories.Implementation.SQL
{
    public class AccountEntity : IAccount
    {
        public string Id { get; set; }
        
        public string ClientId { get; set; }
        
        public string TradingConditionId { get; set; }
        
        public string BaseAssetId { get; set; }

        public decimal Balance { get; set; }

        public decimal WithdrawTransferLimit { get; set; }
        
        public string LegalEntity { get; set; }

        public bool IsDisabled { get; set; }
        
        public bool IsWithdrawalDisabled { get; set; }

        public DateTime ModificationTimestamp { get; set; }

        List<TemporaryCapital> IAccount.TemporaryCapital => JsonConvert.DeserializeObject<List<TemporaryCapital>>(TemporaryCapital);
        public string TemporaryCapital { get; set; } = "[]";

        List<string> IAccount.LastExecutedOperations => JsonConvert.DeserializeObject<List<string>>(LastExecutedOperations);
        public string LastExecutedOperations { get; set; } = "[]";
    }
}