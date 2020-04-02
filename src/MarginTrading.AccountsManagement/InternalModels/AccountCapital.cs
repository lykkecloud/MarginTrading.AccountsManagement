// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.AccountsManagement.InternalModels
{
    public readonly struct AccountCapital
    {
        public decimal Value { get; }
        
        public decimal Temporary { get; }
        
        public decimal Compensations { get; }
        
        public decimal Disposable { get; }
        
        public string AssetId { get; }

        private const string NegativeValueErrorMessage = "Value can't be negative";

        public AccountCapital(decimal value, decimal temporary, decimal compensations, string assetId)
        {
            if (value < 0)
                throw new ArgumentException(NegativeValueErrorMessage, nameof(value));
            
            if (temporary < 0)
                throw new ArgumentException(NegativeValueErrorMessage, nameof(temporary));
            
            if (compensations < 0)
                throw new ArgumentException(NegativeValueErrorMessage, nameof(compensations));
            
            if (value < (temporary + compensations))
                throw new ArgumentException("Temporary capital and compensations can't be greater than capital in total");
            
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ArgumentNullException(nameof(assetId));
            
            Value = value;
            Temporary = temporary;
            Compensations = compensations;
            AssetId = assetId;
            
            Disposable = value - temporary - compensations;
        }
    }
}