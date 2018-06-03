using System;
using System.ComponentModel;
using JetBrains.Annotations;
using MarginTrading.AccountsManagement.Contracts.Models;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Something in the account has changed or the account has been created
    /// </summary>
    [MessagePackObject]
    public class AccountChangedEvent
    {
        public AccountChangedEvent(DateTime changeTimestamp, [NotNull] AccountContract account,
            AccountChangedEventTypeContract eventType)
        {
            if (!Enum.IsDefined(typeof(AccountChangedEventTypeContract), eventType))
                throw new InvalidEnumArgumentException(
                    nameof(eventType),
                    (int) eventType,
                    typeof(AccountChangedEventTypeContract));

            if (changeTimestamp == default(DateTime))
                throw new ArgumentOutOfRangeException(nameof(changeTimestamp));

            ChangeTimestamp = changeTimestamp;
            Account = account ?? throw new ArgumentNullException(nameof(account));
            EventType = eventType;
        }

        /// <summary>
        /// Date and time of event
        /// </summary>
        [Key(0)]
        public DateTime ChangeTimestamp { get; }

        /// <summary>
        /// Account snapshot at the moment immediately after the event happened
        /// </summary>
        [NotNull]
        [Key(1)]
        public AccountContract Account { get; }

        /// <summary>
        /// What happend to the account
        /// </summary>
        [Key(2)]
        public AccountChangedEventTypeContract EventType { get; }
    }
}