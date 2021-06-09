// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Events
{
    /// <summary>
    /// Event is produced when negative protection is launched on any account.
    /// </summary>
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// ============================= MODIFICATION RESTRICTION NOTE =========================================
    /// Please note, this contract modification has to go through confirmation procedure with BNPP since it
    /// is used for notification purposes and there is a source code which deserializes this kind of messages 
    /// ====================================== END OF NOTE ==================================================
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
    [MessagePackObject]
    public class NegativeProtectionEvent
    {
        [NotNull]
        [Key(0)]
        public string Id { get; }
        
        [NotNull]
        [Key(1)]
        public string CorrelationId { get; }
        
        [NotNull]
        [Key(2)]
        public string CausationId { get; }
        
        [Key(3)]
        public DateTime EventTimestamp { get; }
        
        [NotNull]
        [Key(4)]
        public string ClientId { get; }
        
        [NotNull]
        [Key(5)]
        public string AccountId { get; }
        
        /// <summary>
        /// Amount of compensation to make account positive
        /// </summary>
        [Key(6)]
        public decimal Amount { get; }
        
        /// <summary>
        /// Shows if negative balance was automatically compensated
        /// </summary>
        [Key(7)]
        public bool IsAutoCompensated { get; }

        public NegativeProtectionEvent([NotNull] string id, [NotNull] string correlationId,
            [NotNull] string causationId, DateTime eventTimestamp, [NotNull] string clientId,
            [NotNull] string accountId, decimal amount, bool isAutoCompensated)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            CausationId = causationId ?? throw new ArgumentNullException(nameof(causationId));
            EventTimestamp = eventTimestamp;
            ClientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount;
            IsAutoCompensated = isAutoCompensated;
        }
    }
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // ============================= MODIFICATION RESTRICTION NOTE =========================================
    // Please note, this contract modification has to go through confirmation procedure with BNPP since it
    // is used for notification purposes and there is a source code which deserializes this kind of messages 
    // ====================================== END OF NOTE ==================================================
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
}