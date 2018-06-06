using System;
using System.Reactive;
using System.Reactive.Linq;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.BitstampAdapter.Services.Settings;

namespace Lykke.Service.BitstampAdapter.Services
{
    public static class OrderBookPipelineExtensions
    {
        public static IObservable<OrderBook> OnlyWithPositiveSpread(this IObservable<OrderBook> source)
        {
            return source.Where(x => !OrderBookExtensions.TryDetectNegativeSpread(x, out _));
        }

        public static IObservable<T> ThrottleEachInstrument<T>(
            this IObservable<T> source,
            Func<T, string> getAsset,
            float maxEventsPerSecond)
        {
            if (maxEventsPerSecond < 0) throw new ArgumentOutOfRangeException(nameof(maxEventsPerSecond));
            if (Math.Abs(maxEventsPerSecond) < 0.01) return source;

            return source
                .GroupBy(getAsset)
                .Select(grouped => grouped.Sample(TimeSpan.FromSeconds(1) / maxEventsPerSecond))
                .Merge();
        }

        public static IObservable<T> DistinctEveryInstrument<T>(this IObservable<T> source, Func<T, string> getAsset)
        {
            return source.GroupBy(getAsset).Select(x => x.DistinctUntilChanged()).Merge();
        }

        public static IObservable<Unit> PublishToRmq<T>(
            this IObservable<T> source,
            PublishSettings rmq,
            ILog log)
        {
            var settings = RabbitMqSubscriptionSettings.CreateForPublisher(
                rmq.ConnectionString,
                rmq.Exchanger);

            var connection
                = new RabbitMqPublisher<T>(settings)
                    .SetLogger(log)
                    .SetSerializer(new JsonMessageSerializer<T>())
                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                    .PublishSynchronously()
                    .Start();

            return source.SelectMany(async (T x) =>
            {
                await connection.ProduceAsync(x);
                return Unit.Default;
            });
        }

        public static IObservable<T> ReportErrors<T>(this IObservable<T> source, ILog log)
        {
            return source.Do(_ => { }, err => log.WriteWarning("", "", "", err));
        }
    }
}