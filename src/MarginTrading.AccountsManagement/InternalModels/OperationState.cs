using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.InternalModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OperationState
    {
        Created = 0,
        Started = 1,
        Succeeded = 2,
        Failed = 3,   
    }
}