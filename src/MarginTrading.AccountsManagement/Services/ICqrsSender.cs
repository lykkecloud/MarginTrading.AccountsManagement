// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.AccountsManagement.Services
{
    /// <summary>
    /// Sends cqrs messages from Account Management contexts
    /// </summary>
    public interface ICqrsSender
    {
        void SendCommandToSelf<T>(T command);
        void PublishEvent<T>(T ev, string boundedContext = null);
    }
}