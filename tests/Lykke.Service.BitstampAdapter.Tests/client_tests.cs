using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Common.PasswordTools;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Lykke.Service.BitstampAdapter.Tests
{
    public sealed class client_tests
    {
        [Test, Explicit]
        public async Task get_balance()
        {
            var apiCredentials = new ApiCredentials(
                "",
                "",
                ""
            );

            var client = new ApiClient(apiCredentials, new LogToConsole());

            Console.WriteLine(JsonConvert.SerializeObject(await client.OrderStatus("1704450591")));
        }

        [Test]
        public void test_decode_shortorder()
        {
            var input = "[\r\n  {\r\n    \"price\": \"10000.00\",\r\n    \"currency_pair\": \"BTC/USD\",\r\n    \"datetime\": \"2018-06-15 14:49:16\",\r\n    \"amount\": \"0.00500000\",\r\n    \"type\": \"1\",\r\n    \"id\": \"1689112506\"\r\n  }\r\n]";

            var decoded = JsonConvert.DeserializeObject<IReadOnlyCollection<ShortOrder>>(input);

            Assert.AreEqual(1, decoded.Count);

            var order = decoded.First();

            Assert.AreEqual(new DateTime(2018, 06, 15, 14, 49, 16, DateTimeKind.Utc), order.Datetime);
        }
    }
}
