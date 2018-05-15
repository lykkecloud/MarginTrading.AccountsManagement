using System.Threading.Tasks;
using MarginTrading.AccountsManagement.Contracts.Messages;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.InternalModels;

namespace MarginTrading.AccountsManagement.Services
{
    public interface IEventSender
    {
        Task SendAccountUpdatedEvent(Account account);
        Task SendAccountCreatedEvent(Account account);
        Task SendAccountHistoryEvent(AccountHistoryContract model);
    }
}