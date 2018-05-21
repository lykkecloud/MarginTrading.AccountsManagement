using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IEventSender
    {
        void SendAccountChangedEvent(Account account, AccountChangedEventTypeContract eventType);
    }
}