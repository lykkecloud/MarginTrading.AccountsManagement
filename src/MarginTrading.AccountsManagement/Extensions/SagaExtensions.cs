// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Extensions
{
    public static class SagaExtensions
    {
        public static bool SwitchState<TState>(this OperationDataBase<TState> data, TState expectedState, TState nextState)
            where TState : struct, IConvertible
        {
            if (data == null)
            {
                throw new InvalidOperationException("Operation execution data was not properly initialized.");
            }
            
            if (Convert.ToInt32(data.State) < Convert.ToInt32(expectedState))
            {
                // Throws to retry and wait until the operation will be in the required state
                throw new InvalidOperationException(
                    $"Operation execution state can't be switched: {data.State} -> {nextState}. Waiting for the {expectedState} state.");
            }

            if (Convert.ToInt32(data.State) > Convert.ToInt32(expectedState))
            {
                // Already in the next state, so this event can be just ignored
                return false;
            }

            data.State = nextState;

            return true;
        }
    }
}