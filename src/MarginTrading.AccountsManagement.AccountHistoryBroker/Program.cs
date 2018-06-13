using MarginTrading.AccountsManagement.BrokerBase;

namespace MarginTrading.AccountsManagement.AccountHistoryBroker
{
    public class Program : WebAppProgramBase<Startup>
    {
        public static void Main(string[] args)
        {
            RunOnPort(5011);
        }
    }
}