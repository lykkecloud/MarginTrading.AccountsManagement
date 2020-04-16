// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public readonly struct AccountCapital
    {
        public decimal Balance { get; }
        
        public decimal Temporary { get; }
        
        public decimal Compensations { get; }
        
        public decimal Disposable { get; }

        public string AssetId { get; }

        public AccountCapital(decimal balance, decimal temporary, decimal compensations, string assetId)
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ArgumentNullException(nameof(assetId));
            
            Balance = balance;
            Temporary = temporary;
            Compensations = compensations;
            AssetId = assetId;
            Disposable = Math.Max(0,
                Balance - (
                    Math.Max(0, Temporary) + 
                    Math.Max(0, Compensations)));
        }
    }
}