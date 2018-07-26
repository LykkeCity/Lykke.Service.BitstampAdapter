using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Server.Settings;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.Services.Settings;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

#pragma warning disable 1998

namespace Lykke.Service.BitstampAdapter.Services
{
    public sealed class OrderbookPublishingService : IHostedService
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly OrderbookSettings _orderbookSettings;
        private readonly RabbitMqSettings _rmqSettings;
        private IDisposable _subscription;
        private readonly IReadOnlyCollection<string> _instruments;
        private const string BitstampName = "bitstamp";

        public OrderbookPublishingService(
            ILogFactory logFactory,
            OrderbookSettings orderbookSettings,
            RabbitMqSettings rmqSettings,
            InstrumentSettings instrumentSettings)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _orderbookSettings = orderbookSettings;
            _rmqSettings = rmqSettings;
            _instruments = instrumentSettings.Orderbooks;
        }

        public OrderBooksSession Session { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.WriteInfo(nameof(OrderbookPublishingService), "",
                $"Listening orderbook for instruments: {string.Join(", ", _instruments)}, " +
                "you can find supported instruments in documentation: https://www.bitstamp.net/websocket/");

            var orderbooks = Observable.Using(
                () => new DisposablePusher("de504dc5763aeef9ff52", _log),
                p => _instruments.Select(i => ProcessOrderBooks(p, i)).Merge());

            Session = orderbooks.FromRawOrderBooks(
                _instruments,
                new OrderBookProcessingSettings
                {
                    AllowedAnomalisticAssets = new string[0],
                    MaxEventPerSecondByInstrument = _orderbookSettings.MaxEventPerSecondByInstrument,
                    OrderBookDepth = 100,
                    OrderBooks = new RmqOutput
                    {
                        ConnectionString = _rmqSettings.OrderBooks.ConnectionString,
                        Durable = false,
                        Enabled = _rmqSettings.OrderBooks.Enabled,
                        Exchanger = _rmqSettings.OrderBooks.Exchanger
                    },
                    TickPrices = new RmqOutput
                    {
                        ConnectionString = _rmqSettings.TickPrices.ConnectionString,
                        Durable = false,
                        Enabled = _rmqSettings.TickPrices.Enabled,
                        Exchanger = _rmqSettings.TickPrices.Exchanger
                    }
                },
                _logFactory);

            _subscription = new CompositeDisposable(
                Session,
                Session.Worker.Subscribe());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
        }

        private IObservable<OrderBook> ProcessOrderBooks(DisposablePusher p, string instrument)
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
}
