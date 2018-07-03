using System;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;

namespace MarginTrading.AccountsManagement.Contracts.Api
{
    /// <summary>
    /// Request to change account's balance
    /// </summary>
    public class AccountChargeManuallyRequest : AccountChargeRequest
    {
        /// <summary>
        /// Balance change reason type
        /// </summary>
        public AccountBalanceChangeReasonTypeContract ReasonType { get; set; }
        
        /// <summary>
        /// Id of linked object
        /// </summary>
        public string EventSourceId { get; set; }
    }
}