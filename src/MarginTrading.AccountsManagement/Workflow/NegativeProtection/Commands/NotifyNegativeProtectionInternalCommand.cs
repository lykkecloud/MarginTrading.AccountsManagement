// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using MessagePack;

namespace MarginTrading.AccountsManagement.Workflow.NegativeProtection.Commands
{
    [MessagePackObject]
    public class NotifyNegativeProtectionInternalCommand
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
        
        [Key(6)]
        public decimal Amount { get; }
        
        [Key(7)]
        public int OpenPositionsRemainingOnAccount { get; }
        
        [Key(8)]
        public decimal CurrentTotalCapital { get; }

        public NotifyNegativeProtectionInternalCommand([NotNull] string id, [NotNull] string correlationId, 
            [NotNull] string causationId, DateTime eventTimestamp, [NotNull] string clientId, 
            [NotNull] string accountId, decimal amount, int openPositionsRemainingOnAccount, decimal currentTotalCapital)
        {
            Id = id;
            CorrelationId = correlationId;
            CausationId = causationId;
            EventTimestamp = eventTimestamp;
            ClientId = clientId;
            AccountId = accountId;
            Amount = amount;
            OpenPositionsRemainingOnAccount = openPositionsRemainingOnAccount;
            CurrentTotalCapital = currentTotalCapital;
        }
    }
}