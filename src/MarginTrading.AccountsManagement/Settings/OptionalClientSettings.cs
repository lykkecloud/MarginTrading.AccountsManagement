// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace MarginTrading.AccountsManagement.Settings
{
    public class OptionalClientSettings
    {
        public string ServiceUrl { get; set; }
        
        [Optional]
        public string ApiKey { get; set; }
    }
}