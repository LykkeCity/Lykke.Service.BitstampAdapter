using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Microsoft.AspNetCore.Connections.Features;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public sealed class ApiClient
    {
        private readonly HttpClient _client;

        public readonly string InternalApiKey;

        private static FormUrlEncodedContent EmptyRequest()
        {
            return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());
        }

        public ApiClient(
            IApiCredentials credentials,
            ILog log,
            string internalApiKey = "n/a")
        {
            InternalApiKey = internalApiKey;

            HttpMessageHandler mainHandler = new LoggingHandler(log, new HttpClientHandler());

            if (credentials != null)
            {
                mainHandler = new AuthenticationHandler(
                    credentials.UserId,
                    credentials.Key,
                    credentials.Secret,
                    mainHandler);
            }

            _client = new HttpClient(mainHandler)
            {
                BaseAddress = new Uri("https://www.bitstamp.net/api/v2/")
            };
        }

        public async Task<IReadOnlyCollection<WalletBalanceModel>> Balance()
        {
            using (var msg = await _client.PostAsync("balance/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);

                if (!(json is JObject obj))
                {
                    throw new BitstampApiException($"Expected Json Object, got {json.Type}");
                }

                var available = GetBalances(obj, "available");
                var balance = GetBalances(obj, "balance");
                var reserved = GetBalances(obj, "reserved");

                var currencies = new HashSet<string>(
                    available.Keys
                        .Concat(balance.Keys)
                        .Concat(reserved.Keys));


                return currencies
                    .Select(x => new WalletBalanceModel
                    {
                        Asset = x,
                        Balance = balance.GetValueOrDefault(x),
                        Reserved = reserved.GetValueOrDefault(x)
                    })
                    .ToArray();
            }
        }

        private static IReadOnlyDictionary<string, decimal> GetBalances(JObject jObject, string suffix)
        {
            var ending = $"_{suffix}";

            return jObject
                .Cast<KeyValuePair<string, JToken>>()
                .Where(x => x.Key.EndsWith(ending))
                .Select(x =>
                {
                    var convertable = true;
                    decimal balance = 0;
                    try
                    {
                        balance = x.Value.Value<decimal>();
                    }
                    catch
                    {
                        convertable = false;
                    }

                    return (x.Key.Substring(0, x.Key.Length - ending.Length), convertable, balance);
                })
                .Where(x => x.Item2)
                .ToDictionary(x => x.Item1, x => x.Item3);
        }

        private static async Task<JToken> ReadAsJson(HttpResponseMessage msg)
        {
            msg.EnsureSuccessStatusCode();
            var json = await msg.Content.ReadAsAsync<JToken>();

            if (json is JObject obj)
            {
                var errorMessage = CheckErrorField(obj) ?? CheckStatusField(obj);

                if (errorMessage != null)
                {
                    if (IsNotFoundError(errorMessage))
                    {
                        throw new OrderNotFoundException(errorMessage);
                    }

                    if (IsBalanceError(errorMessage))
                    {
                        throw new InsufficientBalanceException(errorMessage);
                    }

                    if (CheckOrderSizeError(errorMessage))
                    {
                        throw new VolumeTooSmallException(errorMessage);
                    }

                    if (CheckOrderPriceError(errorMessage))
                    {
                        throw new InvalidOrderPriceException(errorMessage);
                    }

                    if (CheckOrderIdError(errorMessage))
                    {
                        throw new InvalidOrderIdException();
                    }

                    throw new BitstampApiException(errorMessage);
                }

                return json;
            }
            else
            {
                return json;
            }
        }

        private static bool CheckOrderIdError(string errorMessage)
        {
            return errorMessage.StartsWith("Invalid order id", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool CheckOrderPriceError(string errorMessage)
        {
            var errors = new[]
            {
                "Ensure that there are no more than",
                "Price is more than"
            };

            return errors.Any(errorMessage.StartsWith);
        }

        private static bool CheckOrderSizeError(string errorMessage)
        {
            var errors = new[]
            {
                "Minimum order size is"
            };

            return errors.Any(errorMessage.StartsWith);
        }

        private static bool IsNotFoundError(string errorMessage)
        {
            return string.Equals("Order not found", errorMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsBalanceError(string errorMessage)
        {
            var patterns = new[]
            {
                @"^You have only [\d\-\.]+ \w+ available. Check your account balance for details.$",
                @"^You need [\d\-\.]+ \w+ to open that order. You have only [\d\-\.]+ \w+ available."
            };

            return patterns.Any(x => Regex.IsMatch(errorMessage, x));

        }

        private static string CheckStatusField(JObject obj)
        {
            if (obj.ContainsKey("status") && obj["status"].Value<string>() == "error")
                return GetErrors(obj["reason"]).FirstOrDefault();
            return null;
        }

        private static IEnumerable<string> GetErrors(JToken jToken)
        {
            if (jToken is JObject obj)
            {
                foreach (var s in obj)
                {
                    if (s.Value is JArray arr)
                    {
                        foreach (var err in arr)
                        {
                            var msg = err.ToString();

                            if (!string.IsNullOrWhiteSpace(msg))
                                yield return msg;
                        }
                    }
                }
            }

            yield return jToken.ToString();
        }

        private static string CheckErrorField(JObject obj)
        {
            if (!obj.ContainsKey("error")) return null;

            var error = obj["error"].Value<string>();

            if (!string.IsNullOrWhiteSpace(error))
            {
                return error;
            }

            return null;
        }

        public async Task<IReadOnlyCollection<ShortOrder>> OpenOrders()
        {
            using (var msg = await _client.PostAsync("open_orders/all/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<IReadOnlyCollection<ShortOrder>>();
            }
        }

        public async Task<PlaceOrderResponse> BuyLimitOrder(PlaceOrderCommand order)
        {
            using (var msg = await _client.PostAsync($"buy/{WebUtility.UrlEncode(order.Asset)}/",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"amount", order.Amount.ToString(CultureInfo.InvariantCulture)},
                    {"price", order.Price.ToString(CultureInfo.InvariantCulture)}
                })))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<PlaceOrderResponse>();
            }
        }

        public async Task<PlaceOrderResponse> SellLimitOrder(PlaceOrderCommand order)
        {
            using (var msg = await _client.PostAsync($"sell/{WebUtility.UrlEncode(order.Asset)}/",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"amount", order.Amount.ToString(CultureInfo.InvariantCulture)},
                    {"price", order.Price.ToString(CultureInfo.InvariantCulture)}
                })))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<PlaceOrderResponse>();
            }
        }

        public async Task<OrderStatusResponse> OrderStatus(string id)
        {
            using (var msg = await _client.PostAsync("order_status/",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"id", id }
                })))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<OrderStatusResponse>();
            }
        }

        public async Task<CancelOrderResponse> CancelOrder(string requestOrderId)
        {
            using (var msg = await _client.PostAsync("cancel_order/",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"id", requestOrderId }
                })))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<CancelOrderResponse>();
            }
        }
    }
}
