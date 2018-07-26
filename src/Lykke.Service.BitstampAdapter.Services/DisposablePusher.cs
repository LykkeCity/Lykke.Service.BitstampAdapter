using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common.Log;
using Newtonsoft.Json.Linq;
using PusherClient;

namespace Lykke.Service.BitstampAdapter.Services
{
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
            _log.WriteInfo(nameof(DisposablePusher), "", "Connected");
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
}
