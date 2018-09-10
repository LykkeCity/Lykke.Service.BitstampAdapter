using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.AzureRepositories;

namespace Lykke.Service.BitstampAdapter.Controllers.Dto
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
}
