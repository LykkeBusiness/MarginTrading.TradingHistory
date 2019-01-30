using System;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Retries;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.TradingHistory.TestClient
{
    internal static class Program
    {
        private static int _counter;
        
        static async Task Main(string[] args)
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
        
        private static async Task Run()
        {
            var retryStrategy = new LinearRetryStrategy(TimeSpan.FromSeconds(10), 50);
            var generator = HttpClientGenerator.BuildForUrl("http://localhost:5040")
                .WithRetriesStrategy(retryStrategy).Create();

            await CheckOrderEventsPaginatedApiAsync(generator);
            //await CheckDealsApiAsync(generator);

            Console.WriteLine("Successfully finished");
        }

        private static async Task CheckOrderEventsPaginatedApiAsync(HttpClientGenerator generator)
        {
            var api = generator.Generate<IOrderEventsApi>();

            var statuses = new[]
            {
                OrderStatusContract.Executed,
                OrderStatusContract.ExecutionStarted,
            }.ToList();
            var someOrderEvents = await api.OrderHistoryByPages(null, null, statuses,
                withRelated: false,
                createdTimeStart: DateTime.Parse("2019-01-27T05:27:02.133"),
                createdTimeEnd: DateTime.Parse("2019-01-28T05:27:02.133"));

            var countFalse = someOrderEvents.Contents.Count(x => !statuses.Contains(x.Status));
            if (countFalse != 0)
            {
                throw new Exception($"Failed items: {countFalse}");
            }
        }

        private static async Task CheckDealsApiAsync(HttpClientGenerator generator)
        {
            var api = generator.Generate<IDealsApi>();
            var deals = await api.List("AA1000", "ADIDAS_AG").Dump();
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

        public static T Dump<T>(this T o)
        {
            var str = o is string s ? s : JsonConvert.SerializeObject(o);
            Console.WriteLine("{0}. {1}", ++_counter, str);
            return o;
        }

        public static async Task<T> Dump<T>(this Task<T> t)
        {
            return (await t).Dump();
        }

        public static async Task Dump(this Task o)
        {
            await o;
            "ok".Dump();
        }
    }
}
