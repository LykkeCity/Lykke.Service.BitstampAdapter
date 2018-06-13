using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.AzureRepositories;
using Lykke.Service.BitstampAdapter.Services.BitstampClient;
using Lykke.Service.BitstampAdapter.Services.BitstampClient.Dsl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.BitstampAdapter.Controllers
{
    public sealed class CreatedLimitOrder : ILimitOrder
    {
        public CreatedLimitOrder(
            string id,
            string instrument,
            decimal price,
            decimal amount,
            DateTime createdUtc,
            DateTime modifiedUtc,
            TradeType tradeType,
            OrderStatus status,
            decimal? avgExecutionPrice,
            decimal executedAmount,
            decimal remainingAmount)
        {
            Id = id;
            Instrument = instrument;
            Price = price;
            Amount = amount;
            CreatedUtc = createdUtc;
            ModifiedUtc = modifiedUtc;
            TradeType = tradeType;
            Status = status;
            AvgExecutionPrice = avgExecutionPrice;
            ExecutedAmount = executedAmount;
            RemainingAmount = remainingAmount;
        }

        public string Id { get; set; }
        public string Instrument { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ModifiedUtc { get; set; }
        public TradeType TradeType { get; set; }
        public OrderStatus Status { get; set; }
        public decimal? AvgExecutionPrice { get; set; }
        public decimal ExecutedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public sealed class SpotController : SpotControllerBase<ApiClient>
    {
        private readonly LimitOrderRepository _limitOrderRepository;

        public SpotController(LimitOrderRepository limitOrderRepository)
        {
            _limitOrderRepository = limitOrderRepository;
        }

        public override async Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            return new GetWalletsResponse
            {
                Wallets = await Api.Balance()
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
                    orderId = (await Api.BuyLimitOrder(limitOrder)).Id;
                    break;

                case TradeType.Sell:
                    orderId = (await Api.SellLimitOrder(limitOrder)).Id;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _limitOrderRepository.Insert(new CreatedLimitOrder(
                id: orderId,
                instrument: limitOrder.Asset,
                price: request.Price,
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
            return new GetLimitOrdersResponse { Orders = (await Api.OpenOrders()).Select(FromShortOrder).ToArray() };
        }

        public override async Task<CancelLimitOrderResponse> CancelLimitOrderAsync(
            [FromBody]CancelLimitOrderRequest request)
        {
            var response = await Api.CancelOrder(request.OrderId);

            await _limitOrderRepository.UpdateStatus(response.Id, OrderStatus.Canceled);

            return new CancelLimitOrderResponse {OrderId = response.Id};
        }

        public override async Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            var response = await Api.OrderStatus(orderId);

            return FromLimitOrder(await _limitOrderRepository.UpdateTransactions(
                orderId,
                ConvertStatus(response.Status),
                x => GetTransactions(x, response.Transactions)
            ));
        }

        private IReadOnlyCollection<OrderTransaction> GetTransactions(
            ILimitOrder order,
            IEnumerable<JObject> transactions)
        {
            var (s1, s2) = GetSymbols(order.Instrument);

            string resultCurrency = s1;

//            switch (order.TradeType)
//            {
//                case TradeType.Buy:
//                    resultCurrency = s1;
//                    break;
//                case TradeType.Sell:
//                    resultCurrency = s2;
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }

            return transactions.Select(tr =>
            {
                var dict = new Dictionary<string, JToken>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var kv in tr)
                {
                    dict[kv.Key] = kv.Value;
                }

                if (!dict.ContainsKey(resultCurrency))
                {
                    throw new InvalidOperationException($"Result currency not found in response: {resultCurrency}");
                }

                return new OrderTransaction
                {
                    Amount = dict[resultCurrency].Value<decimal>(),
                    Price = dict["price"].Value<decimal>()
                };
            }).ToArray();
        }

        private (string, string) GetSymbols(string orderInstrument)
        {
            if (orderInstrument.Length != 6) throw new ArgumentException(
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
                Timestamp = limitOrder.ModifiedUtc,
                TradeType = limitOrder.TradeType
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
