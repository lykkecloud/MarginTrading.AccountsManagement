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