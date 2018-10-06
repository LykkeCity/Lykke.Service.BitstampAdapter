using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Fails;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.AzureRepositories;
using Lykke.Service.BitstampAdapter.AzureRepositories.Models;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    public sealed class SpotController : SpotControllerBase<ApiClient>
    {
        private readonly LimitOrderRepository _limitOrderRepository;
        private ILog _log;

        public SpotController(LimitOrderRepository limitOrderRepository, ILogFactory logFactory)
        {
            _limitOrderRepository = limitOrderRepository;
            _log = logFactory.CreateLog(this);
        }

        public override async Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            return new GetWalletsResponse
            {
                Wallets = await Api.GetBalanceAsync()
            };
        }

        public override async Task<OrderIdResponse> CreateLimitOrderAsync([FromBody] LimitOrderRequest request)
        {
            var limitOrder = new PlaceOrderCommand
            {
                Price = request.Price,
                Amount = request.Volume,
                Asset = request.Instrument
            };

            string orderId;

            switch (request.TradeType)
            {
                case TradeType.Buy:
                    orderId = (await Api.CreateBuyLimitOrderAsync(limitOrder)).Id;
                    break;

                case TradeType.Sell:
                    orderId = (await Api.CreateSellLimitOrderAsync(limitOrder)).Id;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _limitOrderRepository.InsertAsync(new LimitOrder(
                orderId,
                limitOrder.Asset,
                request.Price,
                executedAmount: 0M,
                amount: request.Volume,
                createdUtc: DateTime.UtcNow,
                modifiedUtc: DateTime.UtcNow,
                tradeType: request.TradeType,
                status: OrderStatus.Active,
                avgExecutionPrice: null,
                remainingAmount: request.Volume), Api.InternalApiKey);

            return new OrderIdResponse {OrderId = orderId};
        }

        public override async Task<GetLimitOrdersResponse> GetLimitOrdersAsync()
        {
            return new GetLimitOrdersResponse
                {Orders = (await Api.GetOpenOrdersAsync()).Select(FromShortOrder).ToArray()};
        }

        public override async Task<CancelLimitOrderResponse> CancelLimitOrderAsync(
            [FromBody] CancelLimitOrderRequest request)
        {
            CancelOrderResponse response;

            try
            {
                response = await Api.CancelOrderAsync(request.OrderId);
            }
            catch (OrderNotFoundException)
            {
                return new CancelLimitOrderResponse {OrderId = request.OrderId};
            }

            await _limitOrderRepository.UpdateStatusAsync(response.Id, OrderStatus.Canceled);

            return new CancelLimitOrderResponse {OrderId = response.Id};
        }

        public override async Task<GetOrdersHistoryResponse> GetOrdersHistoryAsync()
        {
            var orders = await _limitOrderRepository.GetAll().Take(500).ToArray();

            return new GetOrdersHistoryResponse {Orders = orders.Select(FromLimitOrder).ToArray()};
        }

        public override async Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            var response = await Api.GetOrderStatusAsync(orderId);

            var limitOrder = await _limitOrderRepository.UpdateTransactionsAsync(
                orderId,
                ConvertStatus(response.Status),
                x => GetTransactions(x, response.Transactions)
            );

            if (limitOrder == null) throw new OrderNotFoundException();

            return FromLimitOrder(limitOrder);
        }

        private IReadOnlyCollection<OrderTransaction> GetTransactions(
            ILimitOrder order,
            IEnumerable<JObject> transactions)
        {
            var (cryptoCurrency, _) = GetSymbols(order.Instrument);


            var result = new List<OrderTransaction>();

            foreach (var transaction in transactions)
            {
                try
                {
                    var dict = new Dictionary<string, JToken>(StringComparer.InvariantCultureIgnoreCase);

                    foreach (var keyValue in transaction)
                    {
                        dict[keyValue.Key] = keyValue.Value;
                    }

                    if (!dict.ContainsKey(cryptoCurrency))
                    {
                        throw new InvalidOperationException($"Result currency not found in response: {cryptoCurrency}");
                    }

                    var tran = new OrderTransaction
                    {
                        Amount = decimal.Parse(dict[cryptoCurrency].Value<string>(),
                            System.Globalization.NumberStyles.Any),
                        Price = decimal.Parse(dict["price"].Value<string>(), System.Globalization.NumberStyles.Any)
                    };

                    result.Add(tran);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, context: $"tran: {transaction}, order: {order.ToJson()}");
                    throw;
                }
            }

            return result;
        }

        private (string, string) GetSymbols(string orderInstrument)
        {
            if (orderInstrument.Length != 6)
                throw new ArgumentException(
                    "Expected asset of 6 chars length",
                    nameof(orderInstrument));

            return (orderInstrument.Substring(0, 3), orderInstrument.Substring(3));
        }

        private OrderModel FromLimitOrder(ILimitOrder limitOrder)
        {
            return new OrderModel
            {
                AvgExecutionPrice = limitOrder.AvgExecutionPrice ?? 0,
                ExecutedVolume = limitOrder.ExecutedAmount,
                ExecutionStatus = limitOrder.Status,
                Id = limitOrder.Id,
                OriginalVolume = limitOrder.Amount,
                Price = limitOrder.Price,
                RemainingAmount = limitOrder.RemainingAmount,
                Symbol = limitOrder.Instrument,
                Timestamp = limitOrder.CreatedUtc,
                TradeType = limitOrder.TradeType,
            };
        }

        private OrderStatus ConvertStatus(BitstampOrderStatus status)
        {
            switch (status)
            {
                case BitstampOrderStatus.Open:
                    return OrderStatus.Active;
                case BitstampOrderStatus.Queue:
                    return OrderStatus.Active;
                case BitstampOrderStatus.Finished:
                    return OrderStatus.Fill;
                case BitstampOrderStatus.Canceled:
                    return OrderStatus.Canceled;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static OrderModel FromShortOrder(ShortOrder @short)
        {
            return new OrderModel
            {
                Id = @short.Id,
                Timestamp = @short.Datetime,
                ExecutionStatus = OrderStatus.Active,
                Price = @short.Price,
                OriginalVolume = @short.Amount,
                TradeType = Convert(@short.Type),
                Symbol = FromBitstampCurrency(@short.CurrencyPair)
            };
        }

        private static string FromBitstampCurrency(string shortCurrencyPair)
        {
            return shortCurrencyPair.Replace("/", "").ToLowerInvariant();
        }

        public static TradeType Convert(BitstampOrderType type)
        {
            switch (type)
            {
                case BitstampOrderType.Buy:
                    return TradeType.Buy;
                case BitstampOrderType.Sell:
                    return TradeType.Sell;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
