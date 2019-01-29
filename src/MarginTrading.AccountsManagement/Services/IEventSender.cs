using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;
using MarginTrading.AccountsManagement.InternalModels.Interfaces;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IEventSender
    {
        void SendAccountChangedEvent(string source, IAccount account, AccountChangedEventTypeContract eventType,
            string operationId, AccountBalanceChangeContract balanceChangeContract = null,
            IAccount previousSnapshot = null);
    }
}