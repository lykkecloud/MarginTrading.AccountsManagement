using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Services
{
    internal interface IEventSender
    {
        void SendAccountChangedEvent(IAccount account, AccountChangedEventTypeContract eventType);
    }
}