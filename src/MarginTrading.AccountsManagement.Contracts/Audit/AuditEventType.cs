// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.Contracts.Audit
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditEventType
    {
        Edition,
        Creation,
        Deletion
    }
}