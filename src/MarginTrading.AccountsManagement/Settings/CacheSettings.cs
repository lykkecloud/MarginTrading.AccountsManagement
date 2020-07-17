// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    public class CacheSettings
    {
        public TimeSpan ExpirationPeriod { get; set; }
        
        [Optional]
        public TimeSpan? AccountCapitalPnlExpirationPeriod { get; set; }
    }
}