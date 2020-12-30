// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public readonly struct AccountCapital
    {
        /// <summary>
        /// The account balance
        /// </summary>
        public decimal Balance { get; }
        
        /// <summary>
        /// The temporary capital
        /// </summary>
        public decimal Temporary { get; }
        
        /// <summary>
        /// The total compensations amount
        /// </summary>
        public decimal Compensations { get; }
        
        /// <summary>
        /// The total realised PnL 
        /// </summary>
        public decimal TotalRealisedPnl { get; }
        
        /// <summary>
        /// The total unrealised PnL 
        /// </summary>
        public decimal TotalUnRealisedPnl { get; }
        
        /// <summary>
        /// The available amount
        /// </summary>
        public decimal Disposable { get; }
        
        /// <summary>
        /// The amount of temporary capital which can be revoked
        /// </summary>
        public decimal CanRevokeAmount { get; }

        /// <summary>
        /// The asset
        /// </summary>
        public string AssetId { get; }

        public AccountCapital(decimal balance, 
            decimal totalRealisedPnl,
            decimal totalUnRealisedPnl,
            decimal temporary,
            decimal compensations,
            string assetId, 
            decimal usedMargin)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ArgumentNullException(nameof(assetId));
            
            Balance = balance;
            Temporary = temporary;
            Compensations = compensations;
            AssetId = assetId;
            TotalRealisedPnl = totalRealisedPnl;
            TotalUnRealisedPnl = totalUnRealisedPnl;

            var balanceProtected = Math.Max(0,
                Balance - (
                    Math.Max(0, Temporary) +
                    Math.Max(0, totalRealisedPnl) +
                    Math.Max(0, totalUnRealisedPnl)));

            Disposable = Math.Max(0, balanceProtected - usedMargin);
                    
            CanRevokeAmount = Math.Max(0,
                Balance - (
                    Math.Max(0, totalRealisedPnl) + 
                    Math.Max(0, totalUnRealisedPnl)));
        }
    }
}