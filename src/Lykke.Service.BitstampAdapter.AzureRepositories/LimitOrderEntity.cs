using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitstampAdapter.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateAlways)]
    public class LimitOrderEntity : AzureTableEntity, ILimitOrder
    {
        string ILimitOrder.Id
        {
            get => RowKey;
            set
            {
                RowKey = ByOrder.GenerateRowKey(value);
                PartitionKey = ByOrder.GeneratePartitionKey(value);
            }
        }

        public static class ByOrder
        {
            public static string GeneratePartitionKey(string orderId)
            {
                var padded = orderId.PadLeft(4, '0');
                return padded.Substring(padded.Length - 4);
            }

            public static string GenerateRowKey(string orderId)
            {
                return orderId;
            }

            public static LimitOrderEntity Create(
                ILimitOrder order,
                string internalApiKey,
                DateTime? createdUtc = null,
                OrderStatus status = OrderStatus.Active)
            {
                var dateTime = createdUtc ?? DateTime.UtcNow;

                var limitOrder = new LimitOrderEntity
                {
                    InternalApiKey = internalApiKey,
                    Instrument = order.Instrument,
                    Price = order.Price,
                    Amount = order.Amount,
                    CreatedUtc = dateTime,
                    ModifiedUtc = dateTime,
                    TradeType = order.TradeType,
                    Status = status,
                    AvgExecutionPrice = null,
                    ExecutedAmount = 0,
                    RemainingAmount = order.Amount
                };

                ((ILimitOrder) limitOrder).Id = order.Id;

                return limitOrder;
            }
        }

        public string InternalApiKey { get; set; }
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
}
