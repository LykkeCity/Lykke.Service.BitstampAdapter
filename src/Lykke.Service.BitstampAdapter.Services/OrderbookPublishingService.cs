using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.BitstampAdapter.Services.Settings;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PusherClient;

#pragma warning disable 1998

namespace Lykke.Service.BitstampAdapter.Services
{
    public sealed class OrderbookPublishingService : IHostedService
    {
        private readonly ILog _log;
        private readonly OrderbookSettings _orderbookSettings;
        private readonly RabbitMqSettings _rmqSettings;
        private IDisposable _subscription;
        private const string BitstampName = "bitstamp";

        public OrderbookPublishingService(
            ILog log,
            OrderbookSettings orderbookSettings,
            RabbitMqSettings rmqSettings)
        {
            _log = log;
            _orderbookSettings = orderbookSettings;
            _rmqSettings = rmqSettings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var orderbooks = Observable.Using(
                    () => new DisposablePusher("de504dc5763aeef9ff52", _log),
                    p => BitstampInstruments.All.Select(i => ProcessOrderBooks(p, i)).Merge())
                .OnlyWithPositiveSpread()
                .Publish()
                .RefCount();

            var workers = new List<IObservable<Unit>> {Observable.Never<Unit>()};

            var window = TimeSpan.FromSeconds(30);

            if (_rmqSettings.OrderBooks.Enabled)
            {
                var receivedStat = orderbooks
                    .WindowCount(window)
                    .Sample(window)
                    .Do(x => _log.WriteInfo(
                        nameof(OrderbookPublishingService),
                        "orderbooks",
                        $"Received {x} events for last {window} from websocket"))
                    .Select(_ => Unit.Default);

                var ob = orderbooks
                    .DistinctEveryInstrument(x => x.Asset)
                    .ThrottleEachInstrument(x => x.Asset, _orderbookSettings.MaxEventPerSecondByInstrument)
                    .PublishToRmq(_rmqSettings.OrderBooks, _log)
                    .ReportErrors(_log).
                    RetryWithBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5))
                    .Publish()
                    .RefCount();

                var publishedStat = ob
                    .WindowCount(window)
                    .Sample(window)
                    .Do(x => _log.WriteInfo(
                        nameof(OrderbookPublishingService),
                        "orderbooks",
                        $"Published {x} events for last {window} to RMQ"))
                    .Select(_ => Unit.Default);

                workers.Add(ob);
                workers.Add(receivedStat);
                workers.Add(publishedStat);
            }

            if (_rmqSettings.TickPrices.Enabled)
            {
                var tp = orderbooks.Select(TickPrice.FromOrderBook)
                    .DistinctEveryInstrument(x => x.Asset)
                    .ThrottleEachInstrument(x => x.Asset, _orderbookSettings.MaxEventPerSecondByInstrument)
                    .PublishToRmq(_rmqSettings.TickPrices, _log)
                    .ReportErrors(_log)
                    .RetryWithBackoff(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5));

                workers.Add(tp);
            }

            _subscription = new CompositeDisposable(workers.Select(x => x.Subscribe()));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
        }

        public IObservable<OrderBook> ProcessOrderBooks(DisposablePusher p, string instrument)
        {
            return p.SubscribeToChannel(GetOrderBooksChannelName(instrument))
                .Select(x => ToOrderBook(x, instrument));
        }

        private static OrderBook ToOrderBook(JObject json, string instrument)
        {
            return new OrderBook(
                source: BitstampName,
                asset: instrument,
                timestamp: json["timestamp"].Value<long>().FromEpoch(),
                asks: json["asks"].Select(x =>
                    new OrderBookItem(x[0].Value<decimal>(), x[1].Value<decimal>())),
                bids: json["bids"].Select(x =>
                    new OrderBookItem(x[0].Value<decimal>(), x[1].Value<decimal>())));
        }

        private string GetOrderBooksChannelName(string instrument)
        {
            if (instrument.Equals("btcusd", StringComparison.InvariantCultureIgnoreCase)) return "order_book";
            return $"order_book_{instrument}";
        }
    }

    public sealed class DisposablePusher : IDisposable
    {
        private readonly ILog _log;
        private readonly Pusher _client;

        public DisposablePusher(string pusherKey, ILog log)
        {
            _log = log;
            _client = new Pusher(pusherKey);

            _log.WriteInfo(nameof(DisposablePusher), "", $"Connecting to application {pusherKey}...");
            _client.Connect();
            _log.WriteInfo(nameof(DisposablePusher), "", $"Connected");
        }

        public void Dispose()
        {
            _client?.Disconnect();
        }

        public IObservable<JObject> SubscribeToChannel(string channelName)
        {
            Channel ch = null;

            return Observable.Create<JObject>(async (obs, ct) =>
                {
                        _log.WriteInfo(nameof(DisposablePusher), channelName, $"Subscribing to {channelName}");
                        ch = _client.Subscribe(channelName);

                        ch.BindAll((s, p) =>
                        {
                            // _log.WriteInfo("MessageArrived", channelName, ((JObject)p).ToString(Formatting.None));
                            obs.OnNext((JObject)p);
                        });

                    var tcs = new TaskCompletionSource<Unit>();
                    ct.Register(r => ((TaskCompletionSource<Unit>) r).SetResult(Unit.Default), tcs);
                    await tcs.Task;
                })
                .Finally(() =>
                {
                    ch?.Unsubscribe();
                    ch?.UnbindAll();
                });
        }
    }

     public static class BitstampInstruments
    {
        public static readonly IReadOnlyCollection<string> All = new[]
        {
            "btcusd",
            "btceur",
            "eurusd",
            "xrpusd",
            "xrpeur",
            "xrpbtc",
            "ltcusd",
            "ltceur",
            "ltcbtc",
            "ethusd",
            "etheur",
            "ethbtc",
            "bchusd",
            "bcheur",
            "bchbtc"
        };
    }
}
