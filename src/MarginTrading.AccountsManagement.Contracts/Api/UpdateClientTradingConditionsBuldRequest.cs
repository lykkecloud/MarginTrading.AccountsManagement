// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class UpdateClientTradingConditionsBulkRequest
    {
        public IEnumerable<Update> Updates { get; set; }
        
        public class Update
        {
            public string ClientId { get; set; }

            public string TradingConditionId { get; set; }
        }
    }
}