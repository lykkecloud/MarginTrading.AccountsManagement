// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    public class UpdateClientTradingConditionsBulkRequest
    {
        /// <summary>
        /// The list of updates
        /// </summary>
        public IEnumerable<Update> Updates { get; set; }
        
        /// <summary>
        /// Name of the user who sent the request
        /// </summary>
        [Required]
        public string Username { get; set; }
        
        public class Update
        {
            public string ClientId { get; set; }

            public string TradingConditionId { get; set; }
        }
    }
}