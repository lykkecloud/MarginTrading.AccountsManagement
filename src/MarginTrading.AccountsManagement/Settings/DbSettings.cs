// Copyright (c) 2019 Lykke Corp.

using JetBrains.Annotations;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        public string StorageMode { get; set; }
        
        public string ConnectionString { get; set; }
        public string LogsConnString { get; set; }
    }
}
