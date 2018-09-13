using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.BitstampAdapter.AzureRepositories.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.BitstampAdapter.AzureRepositories
{
    public sealed class LimitOrderRepository
    {
        private readonly INoSQLTableStorage<LimitOrderEntity> _storage;

        public LimitOrderRepository(INoSQLTableStorage<LimitOrderEntity> storage)
        {
            _storage = storage;
        }

        public Task InsertAsync(ILimitOrder order, string internalApiKey)
        {
            return _storage.InsertAsync(LimitOrderEntity.ByOrder.Create(order, internalApiKey));
        }

        public async Task<ILimitOrder> GetByIdAsync(string orderId)
        {
            return await _storage.GetDataAsync(
                LimitOrderEntity.ByOrder.GeneratePartitionKey(orderId),
                LimitOrderEntity.ByOrder.GenerateRowKey(orderId));
        }

        public async Task<ILimitOrder> UpdateStatusAsync(string orderId, OrderStatus status, DateTime? modified = null)
        {
            return await _storage.MergeAsync(
                LimitOrderEntity.ByOrder.GeneratePartitionKey(orderId),
                LimitOrderEntity.ByOrder.GenerateRowKey(orderId),
                x =>
                {
                    if (status == OrderStatus.Canceled)
                        x.Status = x.AvgExecutionPrice == null ? OrderStatus.Canceled : OrderStatus.Fill;
                    else
                        x.Status = status;

                    x.ModifiedUtc = modified ?? DateTime.UtcNow;
                    
                    return x;
                });
        }

        public async Task<ILimitOrder> UpdateTransactionsAsync(
            string orderId,
            OrderStatus status,
            Func<ILimitOrder, IReadOnlyCollection<OrderTransaction>> getTransactions,
            DateTime? modified = null)
        {
            return await _storage.MergeAsync(
                LimitOrderEntity.ByOrder.GeneratePartitionKey(orderId),
                LimitOrderEntity.ByOrder.GenerateRowKey(orderId),
                order =>
                {
                    var transactions = getTransactions(order);

                    order.Status = status;
                    order.ModifiedUtc = modified ?? DateTime.UtcNow;
                    order.ExecutedAmount = transactions.Sum(x => x.Amount);
                    order.RemainingAmount = order.Amount - order.ExecutedAmount;

                    if (order.ExecutedAmount != 0)
                        order.AvgExecutionPrice = transactions.Sum(x => x.Amount * x.Price) / order.ExecutedAmount;

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
                        new TableQuery<LimitOrderEntity>(), info);

                    foreach (var r in result)
                        obs.OnNext(r);

                    info = result.PagingInfo;

                    if (info.NextPage == null)
                        break;
                }

                obs.OnCompleted();
            });
        }
    }
}
