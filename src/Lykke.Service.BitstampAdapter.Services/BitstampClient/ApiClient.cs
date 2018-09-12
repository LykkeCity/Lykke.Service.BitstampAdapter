using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public sealed class ApiClient
    {
        private readonly HttpClient _client;
        private readonly HttpClient _clientV1;

        public readonly string InternalApiKey;

        private static FormUrlEncodedContent EmptyRequest()
        {
            return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>());
        }

        public ApiClient(
            IApiCredentials credentials,
            ILogFactory logFactory,
            string internalApiKey = "n/a")
        {
            var log = logFactory.CreateLog(this);

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

            _clientV1 = new HttpClient(mainHandler)
            {
                BaseAddress = new Uri("https://www.bitstamp.net/api/")
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

            if (msg.StatusCode != HttpStatusCode.Forbidden)
            {
                msg.EnsureSuccessStatusCode();
            }

            var json = await msg.Content.ReadAsAsync<JToken>();

            if (json is JObject obj)
            {
                var errorMessage = CheckErrorField(obj) ?? CheckStatusField(obj);

                if (errorMessage != null)
                {
                    if (msg.StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new BitstampApiException(errorMessage);
                    }

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
            return errorMessage.StartsWith("Order not found", StringComparison.InvariantCultureIgnoreCase);
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

        public async Task<TransferResult> TransferSubToMain(string subAccount, decimal amount, string currency)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"currency", currency}
            };

            if (!string.IsNullOrEmpty(subAccount))
            {
                prm.Add("subAccount", subAccount);
            }
            
            using (var msg = await _client.PostAsync("transfer-to-main/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<TransferResult>();
            }
        }

        public async Task<TransferResult> TransferMainToSub(string subAccount, decimal amount, string currency)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"currency", currency},
                { "subAccount", subAccount }
            };

            using (var msg = await _client.PostAsync("transfer-from-main/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                return json.ToObject<TransferResult>();
            }
        }

        public async Task<List<UnconfirmedBitcoinDeposit>> UnconfirmedBitcoinDeposits()
        {
            using (var msg = await _clientV1.PostAsync("unconfirmed_btc/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<List<UnconfirmedBitcoinDeposit>>(str);
                return res;
            }
        }

        public async Task<List<Withdrawal>> WithdrawalRequests(int timedelta)
        {
            if (timedelta > 50000000) timedelta = 50000000;
            if (timedelta <= 0) timedelta = 86400;

            var prm = new Dictionary<string, string>
            {
                {"timedelta", timedelta.ToString(CultureInfo.InvariantCulture)}
            };

            using (var msg = await _client.PostAsync("withdrawal-requests/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();

                try
                {
                    var res = JsonConvert.DeserializeObject<List<Withdrawal>>(str);
                    return res;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task<string> BitcoinDepositAddress()
        {
            using (var msg = await _clientV1.PostAsync("bitcoin_deposit_address/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var res = json.ToString();
                return res;
            }
        }

        public async Task<string> LitecoinDepositAddress()
        {
            using (var msg = await _client.PostAsync("ltc_address/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var res = JsonConvert.DeserializeObject<WalletAddress>(json.ToString());
                return res.Address;
            }
        }

        public async Task<string> EthDepositAddress()
        {
            using (var msg = await _client.PostAsync("eth_address/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var res = JsonConvert.DeserializeObject<WalletAddress>(json.ToString());
                return res.Address;
            }
        }

        public async Task<string> XrpDepositAddress()
        {
            using (var msg = await _client.PostAsync("xrp_address/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var res = JsonConvert.DeserializeObject<WalletAddress>(json.ToString());
                return res.Address;
            }
        }

        public async Task<string> BchDepositAddress()
        {
            using (var msg = await _client.PostAsync("bch_address/", EmptyRequest()))
            {
                var json = await ReadAsJson(msg);
                var res = JsonConvert.DeserializeObject<WalletAddress>(json.ToString());
                return res.Address;
            }
        }

        public async Task<WithdrawalId> CreateBitcoinWithdrawal(decimal amount, string address, bool supportBitGo)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address},
                {"instant", supportBitGo ? "1" : "0"}
            };

            using (var msg = await _clientV1.PostAsync("bitcoin_withdrawal/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<WithdrawalId>(str);
                return res;
            }
        }

        public async Task<WithdrawalId> CreateLitecoinWithdrawal(decimal amount, string address)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address}
            };

            using (var msg = await _client.PostAsync("ltc_withdrawal/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<WithdrawalId>(str);
                return res;
            }
        }

        public async Task<WithdrawalId> CreateEthWithdrawal(decimal amount, string address)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address}
            };

            using (var msg = await _client.PostAsync("eth_withdrawal/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<WithdrawalId>(str);
                return res;
            }
        }

        public async Task<WithdrawalId> CreateXrpWithdrawal(decimal amount, string address, string destinationTag=null)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address},
            };

            if (!string.IsNullOrEmpty(destinationTag))
                 prm.Add("destination_tag", destinationTag);

            using (var msg = await _client.PostAsync("xrp_withdrawal/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<WithdrawalId>(str);
                return res;
            }
        }

        public async Task<WithdrawalId> CreateBchWithdrawal(decimal amount, string address)
        {
            var prm = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address}
            };

            using (var msg = await _client.PostAsync("bch_withdrawal/", new FormUrlEncodedContent(prm)))
            {
                var json = await ReadAsJson(msg);
                var str = json.ToString();
                var res = JsonConvert.DeserializeObject<WithdrawalId>(str);
                return res;
            }
        }
    }
}
