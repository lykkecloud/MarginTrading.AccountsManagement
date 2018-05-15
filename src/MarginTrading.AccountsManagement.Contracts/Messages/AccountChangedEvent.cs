using System;
using MarginTrading.AccountsManagement.Contracts.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.Contracts.Messages
{
    public class AccountChangedEvent
    {
        /// <summary>
        /// Date and time of event
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Account snapshot at the moment immediately after the event happened
        /// </summary>
        public AccountContract Account { get; set; }
        
        /// <summary>
        /// What happend to the account
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountChangedEventType EventType { get; set; }
    }
}