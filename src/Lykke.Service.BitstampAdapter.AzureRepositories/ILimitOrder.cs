using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.BitstampAdapter.AzureRepositories
{
    public interface ILimitOrder
    {
        string Id { get; set; }
        
        string Instrument { get; set; }
        
        decimal Price { get; set; }
        
        decimal Amount { get; set; }
        
        DateTime CreatedUtc { get; set; }
        
        DateTime ModifiedUtc { get; set; }
        
        TradeType TradeType { get; set; }
        
        OrderStatus Status { get; set; }
        
        decimal? AvgExecutionPrice { get; set; }
        
        decimal ExecutedAmount { get; set; }
        
        decimal RemainingAmount { get; set; }
    }
}
