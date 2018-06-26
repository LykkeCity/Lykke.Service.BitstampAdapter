using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.AzureRepositories.Entities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitstampAdapter.AzureRepositories
{
    public struct OrderTransaction
    {
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }

    public sealed class LimitOrderRepository
    {
        private readonly INoSQLTableStorage<LimitOrder> _storage;

        public LimitOrderRepository(INoSQLTableStorage<LimitOrder> storage)
        {
            _storage = storage;
        }

        public Task Insert(ILimitOrder order, string internalApiKey)
        {
            return _storage.InsertAsync(LimitOrder.ByOrder.Create(order, internalApiKey));
        }

        public async Task<ILimitOrder> GetById(string orderId)
        {
            return await _storage.GetDataAsync(
                LimitOrder.ByOrder.GeneratePartitionKey(orderId),
                LimitOrder.ByOrder.GenerateRowKey(orderId));
        }

        public async Task<ILimitOrder> UpdateStatus(string orderId, OrderStatus status, DateTime? modified = null)
        {
            return await _storage.MergeAsync(
                LimitOrder.ByOrder.GeneratePartitionKey(orderId),
                LimitOrder.ByOrder.GenerateRowKey(orderId),
                x =>
                {
                    if (status == OrderStatus.Canceled)
                    {
                        x.Status = x.AvgExecutionPrice == null ? OrderStatus.Canceled : OrderStatus.Fill;
                    }
                    else
                    {
                        x.Status = status;
                    }

                    x.ModifiedUtc = modified ?? DateTime.UtcNow;
                    return x;
                });
        }

        public async Task<ILimitOrder> UpdateTransactions(
            string orderId,
            OrderStatus status,
            Func<ILimitOrder, IReadOnlyCollection<OrderTransaction>> getTransactions,
            DateTime? modified = null)
        {
            return await _storage.MergeAsync(
                LimitOrder.ByOrder.GeneratePartitionKey(orderId),
                LimitOrder.ByOrder.GenerateRowKey(orderId),
                order =>
                {
                    var transactions = getTransactions(order);

                    order.Status = status;
                    order.ModifiedUtc = modified ?? DateTime.UtcNow;
                    order.ExecutedAmount = transactions.Sum(x => x.Amount);
                    order.RemainingAmount = order.Amount - order.ExecutedAmount;

                    if (order.ExecutedAmount != 0)
                    {
                        order.AvgExecutionPrice = transactions.Sum(x => x.Amount * x.Price) / order.ExecutedAmount;
                    }

                    return order;
                });
        }

        public IObservable<ILimitOrder> GetAll()
        {
            return Observable.Create<ILimitOrder>(async (obs, ct) =>
            {
                var info = new PagingInfo {ElementCount = 100};

                while (!ct.IsCancellationRequested)
                {
                    var result = await _storage.ExecuteQueryWithPaginationAsync(
                        new TableQuery<LimitOrder>(), info);

                    foreach (var r in result)
                    {
                        obs.OnNext(r);
                    }

                    info = result.PagingInfo;

                    if (info.NextPage == null) break;
                }

                obs.OnCompleted();
            });
        }
    }

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
