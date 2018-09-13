using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl.Transfer;
using Newtonsoft.Json.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Lykke.Service.BitstampAdapter.Services.BitstampClient
{
    public sealed class ApiClient
    {
        private readonly IApiCredentials _credentials;
        private readonly HttpClientFactory _httpClientFactory;
        public readonly string InternalApiKey;

        public ApiClient(
            IApiCredentials credentials,
            HttpClientFactory httpClientFactory,
            string internalApiKey = "n/a")
        {
            _credentials = credentials;
            _httpClientFactory = httpClientFactory;
            InternalApiKey = internalApiKey;
        }

        public async Task<IReadOnlyCollection<WalletBalanceModel>> GetBalanceAsync()
        {
            JToken data = await GetDataAsync("balance/", _httpClientFactory.GetClient(_credentials, InternalApiKey));

            if (!(data is JObject obj))
                throw new BitstampApiException($"Expected Json Object, got {data.Type}");

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

        public async Task<IReadOnlyCollection<ShortOrder>> GetOpenOrdersAsync()
        {
            JToken data = await GetDataAsync("open_orders/all/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey));

            return data.ToObject<IReadOnlyCollection<ShortOrder>>();
        }

        public async Task<PlaceOrderResponse> CreateBuyLimitOrderAsync(PlaceOrderCommand order)
        {
            JToken data = await GetDataAsync($"buy/{WebUtility.UrlEncode(order.Asset)}/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey),
                new Dictionary<string, string>
                {
                    {"amount", order.Amount.ToString(CultureInfo.InvariantCulture)},
                    {"price", order.Price.ToString(CultureInfo.InvariantCulture)}
                });

            return data.ToObject<PlaceOrderResponse>();
        }

        public async Task<PlaceOrderResponse> CreateSellLimitOrderAsync(PlaceOrderCommand order)
        {
            JToken data = await GetDataAsync($"sell/{WebUtility.UrlEncode(order.Asset)}/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey),
                new Dictionary<string, string>
                {
                    {"amount", order.Amount.ToString(CultureInfo.InvariantCulture)},
                    {"price", order.Price.ToString(CultureInfo.InvariantCulture)}
                });

            return data.ToObject<PlaceOrderResponse>();
        }

        public async Task<OrderStatusResponse> GetOrderStatusAsync(string id)
        {
            JToken data = await GetDataAsync("order_status/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey),
                new Dictionary<string, string>
                {
                    {"id", id}
                });

            return data.ToObject<OrderStatusResponse>();
        }

        public async Task<CancelOrderResponse> CancelOrderAsync(string requestOrderId)
        {
            JToken data = await GetDataAsync("cancel_order/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey),
                new Dictionary<string, string>
                {
                    {"id", requestOrderId}
                });

            return data.ToObject<CancelOrderResponse>();
        }

        public async Task<TransferResult> TransferSubToMainAsync(string subAccount, decimal amount, string currency)
        {
            var parameters = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"currency", currency}
            };

            if (!string.IsNullOrEmpty(subAccount))
                parameters.Add("subAccount", subAccount);

            JToken data = await GetDataAsync("transfer-to-main/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey), parameters);

            return data.ToObject<TransferResult>();
        }

        public async Task<TransferResult> TransferMainToSubAsync(string subAccount, decimal amount, string currency)
        {
            JToken data = await GetDataAsync("transfer-from-main/",
                _httpClientFactory.GetClient(_credentials, InternalApiKey), new Dictionary<string, string>
                {
                    {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                    {"currency", currency},
                    {"subAccount", subAccount}
                });

            return data.ToObject<TransferResult>();
        }

        public async Task<IReadOnlyCollection<UnconfirmedBitcoinDeposit>> GetUnconfirmedBitcoinDepositsAsync()
        {
            JToken data = await GetDataAsync("unconfirmed_btc/",
                _httpClientFactory.GetClientV1(_credentials, InternalApiKey));

            return JsonConvert.DeserializeObject<List<UnconfirmedBitcoinDeposit>>(data.ToString());
        }

        public async Task<IReadOnlyCollection<Withdrawal>> GetWithdrawalRequestsAsync(DateTime fromDate)
        {
            int totalSeconds = (int) (DateTime.UtcNow - fromDate).TotalSeconds;

            if (totalSeconds > 50000000)
                totalSeconds = 50000000;

            if (totalSeconds <= 0)
                totalSeconds = 86400;

            JToken data = await GetDataAsync("withdrawal-requests/",
                _httpClientFactory.GetClientV1(_credentials, InternalApiKey), new Dictionary<string, string>
                {
                    {"timedelta", totalSeconds.ToString(CultureInfo.InvariantCulture)}
                });

            return JsonConvert.DeserializeObject<List<Withdrawal>>(data.ToString());
        }

        public Task<string> GetBitcoinDepositAddressAsync()
            => GetAddressAsync("bitcoin_deposit_address/", _httpClientFactory.GetClientV1(_credentials, InternalApiKey),
                false);

        public Task<string> GetLitecoinDepositAddressAsync()
            => GetAddressAsync("ltc_address/", _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<string> GetEthDepositAddressAsync()
            => GetAddressAsync("eth_address/", _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<string> GetXrpDepositAddressAsync()
            => GetAddressAsync("xrp_address/", _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<string> GetBchDepositAddressAsync()
            => GetAddressAsync("bch_address/", _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<WithdrawalId> CreateBitcoinWithdrawalAsync(decimal amount, string address, bool supportBitGo)
            => CreateWithdrawalAsync("bitcoin_withdrawal/", amount, address,
                _httpClientFactory.GetClientV1(_credentials, InternalApiKey),
                new Tuple<string, string>("instant", supportBitGo ? "1" : "0"));

        public Task<WithdrawalId> CreateLitecoinWithdrawalAsync(decimal amount, string address)
            => CreateWithdrawalAsync("ltc_withdrawal/", amount, address,
                _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<WithdrawalId> CreateEthWithdrawalAsync(decimal amount, string address)
            => CreateWithdrawalAsync("eth_withdrawal/", amount, address,
                _httpClientFactory.GetClient(_credentials, InternalApiKey));

        public Task<WithdrawalId> CreateXrpWithdrawalAsync(decimal amount, string address,
            string destinationTag = null)
        {
            if (!string.IsNullOrEmpty(destinationTag))
            {
                return CreateWithdrawalAsync("xrp_withdrawal/", amount, address,
                    _httpClientFactory.GetClient(_credentials, InternalApiKey),
                    new Tuple<string, string>("destination_tag", destinationTag));
            }

            return CreateWithdrawalAsync("xrp_withdrawal/", amount, address,
                _httpClientFactory.GetClient(_credentials, InternalApiKey));
        }

        public Task<WithdrawalId> CreateBchWithdrawalAsync(decimal amount, string address)
            => CreateWithdrawalAsync("bch_withdrawal/", amount, address,
                _httpClientFactory.GetClient(_credentials, InternalApiKey));

        private static async Task<string> GetAddressAsync(string path, HttpClient client, bool parseResponse = true)
        {
            JToken data = await GetDataAsync(path, client);

            if (!parseResponse)
                return data.ToString();

            WalletAddress walletAddress = JsonConvert.DeserializeObject<WalletAddress>(data.ToString());

            return walletAddress.Address;
        }

        private static async Task<WithdrawalId> CreateWithdrawalAsync(string path, decimal amount, string address,
            HttpClient client, params Tuple<string, string>[] values)
        {
            var parameters = new Dictionary<string, string>
            {
                {"amount", amount.ToString(CultureInfo.InvariantCulture)},
                {"address", address}
            };

            foreach (Tuple<string, string> value in values)
                parameters.Add(value.Item1, value.Item2);

            JToken data = await GetDataAsync(path, client, parameters);

            return JsonConvert.DeserializeObject<WithdrawalId>(data.ToString());
        }

        private static IReadOnlyDictionary<string, decimal> GetBalances(JObject data, string suffix)
        {
            string ending = $"_{suffix}";

            return data.Cast<KeyValuePair<string, JToken>>()
                .Where(x => x.Key.EndsWith(ending))
                .Select(x =>
                {
                    bool convertible = true;

                    decimal balance = 0;

                    try
                    {
                        balance = x.Value.Value<decimal>();
                    }
                    catch
                    {
                        convertible = false;
                    }

                    return (x.Key.Substring(0, x.Key.Length - ending.Length), convertible, balance);
                })
                .Where(x => x.Item2)
                .ToDictionary(x => x.Item1, x => x.Item3);
        }

        private static async Task<JToken> GetDataAsync(string path, HttpClient client,
            Dictionary<string, string> values = null)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(values ?? new Dictionary<string, string>());

            using (HttpResponseMessage responseMessage = await client.PostAsync(path, content))
            {
                return await ReadAsJsonAsync(responseMessage);
            }
        }

        private static async Task<JToken> ReadAsJsonAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.StatusCode != HttpStatusCode.Forbidden)
                responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsAsync<JToken>();

            if (json is JObject data)
            {
                string errorMessage = GetErrorMessage(data);

                if (errorMessage != null)
                {
                    if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                        throw new BitstampApiException(errorMessage);

                    if (IsNotFoundError(errorMessage))
                        throw new OrderNotFoundException(errorMessage);

                    if (IsBalanceError(errorMessage))
                        throw new InsufficientBalanceException(errorMessage);

                    if (IsOrderSizeError(errorMessage))
                        throw new VolumeTooSmallException(errorMessage);

                    if (IsOrderPriceError(errorMessage))
                        throw new InvalidOrderPriceException(errorMessage);

                    if (IsOrderIdError(errorMessage))
                        throw new InvalidOrderIdException();

                    throw new BitstampApiException(errorMessage);
                }
            }

            return json;
        }

        private static string GetErrorMessage(JObject data)
        {
            string errorMessage = null;

            if (data.ContainsKey("error"))
            {
                var error = data["error"].Value<string>();

                if (!string.IsNullOrWhiteSpace(error))
                    errorMessage = error;
            }
            else if (data.ContainsKey("status") && data["status"].Value<string>() == "error")
            {
                errorMessage = GetErrors(data["reason"]).FirstOrDefault();
            }

            return errorMessage;
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

        private static bool IsOrderSizeError(string errorMessage)
        {
            return errorMessage.StartsWith("Minimum order size is", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsOrderPriceError(string errorMessage)
        {
            var errors = new[]
            {
                "Ensure that there are no more than",
                "Price is more than"
            };

            return errors.Any(errorMessage.StartsWith);
        }

        private static bool IsOrderIdError(string errorMessage)
        {
            return errorMessage.StartsWith("Invalid order id", StringComparison.InvariantCultureIgnoreCase);
        }

        private static IEnumerable<string> GetErrors(JToken jToken)
        {
            if (jToken is JObject data)
            {
                foreach (var s in data)
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
    }
}
