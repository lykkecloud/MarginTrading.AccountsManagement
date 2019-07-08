// Copyright (c) 2019 Lykke Corp.

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