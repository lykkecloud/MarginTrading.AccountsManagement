// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common.Log;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AccountsManagement.Settings;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.AccountsManagement.TestClient
{
    /// <summary>
    /// Simple way to check api clients are working.
    /// In future this could be turned into a functional testing app.
    /// </summary>
    internal static class Program
    {
        private static int _counter;

        static async Task Main()
        {
            try
            {
                await Run();
            }
            catch (ApiException e)
            {
                var str = e.Content;
                if (str.StartsWith('"'))
                {
                    str = TryDeserializeToString(str);
                }

                Console.WriteLine(str);
                Console.WriteLine(e.ToAsyncString());
            }
        }

        private static string TryDeserializeToString(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(str);
            }
            catch
            {
                return str;
            }
        }

        private static async Task Run()
        {
//            var clientGenerator = HttpClientGenerator.BuildForUrl("http://localhost:5000")
//                .WithApiKey("TestClient")
//                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromSeconds(10), 12))
//                .Create();
//            
//            await CheckAccountsBalabceHistoryApiWorking(clientGenerator);
//            // todo check other apis

            await CheckBrokerRetries();

            Console.WriteLine("Successfuly finished");
        }

        private static async Task CheckBrokerRetries()
        {
            var logger = new LogToConsole();
            
            var cqrsEngine = new CqrsFake(new CqrsSettings
            {
                ConnectionString = "rabbit connstr here",
                ContextNames = new CqrsContextNamesSettings(),
                EnvironmentName = "andreev",
                RetryDelay = TimeSpan.FromSeconds(5),
            }, logger).CreateEngine();

            logger.WriteLine("waiting 5 sec for cqrsEngine");
            Thread.Sleep(5000);

            cqrsEngine.PublishEvent(new AccountChangedEvent(
                DateTime.UtcNow, 
                "tetest1",
                new AccountContract("","","","",default,default,"",default,default,default,default), 
                AccountChangedEventTypeContract.BalanceUpdated,
                new AccountBalanceChangeContract(
                    "tetetetest1",
                    DateTime.UtcNow, 
                    Enumerable.Repeat("t", 200).Aggregate((f, s) => $"{f}{s}"),//field has length of 64 
                    "tetest1",
                    1,
                    1,
                    10000,
                    "tetest1",
                    AccountBalanceChangeReasonTypeContract.Manual,
                    "tetest1",
                    "tetest1",
                    "tetest1",
                    "tetest1",
                    DateTime.MinValue
                ),
                null,
                null), new CqrsContextNamesSettings().AccountsManagement);
        }

        private static async Task CheckAccountsApiWorking(IHttpClientGenerator clientGenerator)
        {
            var client = clientGenerator.Generate<IAccountsApi>();
            await client.List().Dump();
            var accountResponse = await client.Create(new CreateAccountRequest
                {ClientId = "client1", TradingConditionId = "tc1", BaseAssetId = "ba1"}).Dump();
            await client.GetByClientAndId("client1", accountResponse.Content.Id).Dump();
            await client.Change("client1", accountResponse.Content.Id,
                new ChangeAccountRequest {IsDisabled = true, TradingConditionId = "tc2"}).Dump();
        }
        
        private static async Task CheckAccountsBalabceHistoryApiWorking(IHttpClientGenerator clientGenerator)
        {
            var client = clientGenerator.Generate<IAccountBalanceHistoryApi>();
            var history = await client.ByAccount("AA0011").Dump();
            var record = history?.FirstOrDefault();
            if (record != null)
            {
                var account = record.Value.Key;
                var historyByAccount = await client.ByAccount(account).Dump();
                var historyByAccountAndEvent = await client.ByAccountAndEventSource(account).Dump();
                var date = record.Value.Value.FirstOrDefault()?.ChangeTimestamp ?? DateTime.UtcNow;
                var balance = await client.GetBalanceOnDate(account, date).Dump();
            }
            
        }

        [CanBeNull]
        public static T Dump<T>(this T o)
        {
            var str = o is string s ? s : JsonConvert.SerializeObject(o);
            Console.WriteLine("{0}. {1}", ++_counter, str);
            return o;
        }

        [ItemCanBeNull]
        public static async Task<T> Dump<T>(this Task<T> t)
        {
            var obj = await t;
            obj.Dump();
            return obj;
        }

        public static async Task Dump(this Task o)
        {
            await o;
            "ok".Dump();
        }
    }
}