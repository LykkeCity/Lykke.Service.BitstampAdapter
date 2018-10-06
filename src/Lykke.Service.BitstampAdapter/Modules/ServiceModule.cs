using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.BitstampAdapter.AzureRepositories;
using Lykke.Service.BitstampAdapter.Services;
using Lykke.Service.BitstampAdapter.Services.Settings;
using Lykke.Service.BitstampAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.BitstampAdapter.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var settings = _appSettings.CurrentValue.BitstampAdapterService;

            builder.RegisterInstance(settings).AsSelf();

            builder.RegisterType<OrderbookPublishingService>()
                .As<IHostedService>()
                .AsSelf()
                .WithParameter(new TypedParameter(typeof(OrderbookSettings), settings.Orderbooks))
                .WithParameter(new TypedParameter(typeof(RabbitMqSettings), settings.RabbitMq))
                .WithParameter(new TypedParameter(typeof(InstrumentSettings), settings.Instruments))
                .SingleInstance();

            builder.Register(ctx =>
                    AzureTableStorage<LimitOrderEntity>.Create(
                        _appSettings.ConnectionString(x => x.BitstampAdapterService.Db.OrdersConnString),
                        "BitstampLimitOrders",
                        ctx.Resolve<ILogFactory>()))
                .As<INoSQLTableStorage<LimitOrderEntity>>()
                .SingleInstance();

            builder.RegisterType<LimitOrderRepository>()
                .SingleInstance()
                .AsSelf();

            builder.RegisterType<HttpClientFactory>()
                .SingleInstance()
                .AsSelf();
        }
    }
}
