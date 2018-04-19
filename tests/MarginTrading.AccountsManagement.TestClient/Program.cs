using System;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Api;
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
            var clientGenerator = HttpClientGenerator.BuildForUrl("http://localhost:5007")
                .WithApiKey("TestClient")
                .WithRetriesStrategy(new LinearRetryStrategy(TimeSpan.FromSeconds(10), 12))
                .Create();
            
            await CheckAccountsApiWorking(clientGenerator);
            // todo check other apis

            Console.WriteLine("Successfuly finished");
        }

        private static async Task CheckAccountsApiWorking(IHttpClientGenerator clientGenerator)
        {
            var client = clientGenerator.Generate<IAccountsApi>();
            await client.List().Dump();
            var account = await client
                .Create("client1", new CreateAccountRequest {TradingConditionId = "tc1", BaseAssetId = "ba1"}).Dump();
            await client.GetByClientAndId("client1", account.Id).Dump();
            await client.Change("client1", account.Id,
                new ChangeAccountRequest() {IsDisabled = true, TradingConditionId = "tc2"}).Dump();
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