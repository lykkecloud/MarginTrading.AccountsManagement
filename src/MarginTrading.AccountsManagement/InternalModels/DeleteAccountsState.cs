// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DeleteAccountsState
    { 
        Initiated,
        
        Started,
        
        MtCoreAccountsBlocked,

        AccountsMarkedAsDeleted,
        
        Finished,
    }
}