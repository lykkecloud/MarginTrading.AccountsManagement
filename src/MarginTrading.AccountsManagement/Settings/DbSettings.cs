﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        public string StorageMode { get; set; }

        public string ConnectionString { get; set; }
        public string LogsConnString { get; set; }

        [Optional]
        public int LongRunningSqlTimeoutSec { get; set; } = 100;
    }
}
