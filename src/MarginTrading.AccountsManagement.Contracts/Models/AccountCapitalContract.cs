// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MessagePack;

namespace MarginTrading.AccountsManagement.Contracts.Models
{
    [MessagePackObject]
    public class AccountCapitalContract
    {
        /// <summary>
        /// The account balance
        /// </summary>
        [Key(0)] 
        public decimal Balance { get; }
        
        /// <summary>
        /// The temporary capital
        /// </summary>
        [Key(1)]
        public decimal Temporary { get; }
        
        /// <summary>
        /// The total compensations amount
        /// </summary>
        [Key(2)]
        public decimal Compensations { get; }
        
        /// <summary>
        /// The total realised PnL 
        /// </summary>
        [Key(3)]
        public decimal TotalRealisedPnl { get; }
        
        /// <summary>
        /// The total unrealised PnL 
        /// </summary>
        [Key(4)]
        public decimal TotalUnRealisedPnl { get; }
        
        /// <summary>
        /// The available amount
        /// </summary>
        [Key(5)]
        public decimal Disposable { get; }
        
        /// <summary>
        /// The amount of temporary capital which can be revoked
        /// </summary>
        [Key(6)]
        public decimal CanRevokeAmount { get; }

        /// <summary>
        /// The asset
        /// </summary>
        [Key(7)]
        public string AssetId { get; }

        public AccountCapitalContract(decimal balance, decimal totalRealisedPnl, decimal totalUnRealisedPnl,
            decimal temporary, decimal compensations, string assetId, decimal disposable, decimal canRevokeAmount)
        {
            Balance = balance;
            TotalRealisedPnl = totalRealisedPnl;
            TotalUnRealisedPnl = totalUnRealisedPnl;
            Temporary = temporary;
            Compensations = compensations;
            AssetId = assetId;
            Disposable = disposable;
            CanRevokeAmount = canRevokeAmount;
        }
    }
}