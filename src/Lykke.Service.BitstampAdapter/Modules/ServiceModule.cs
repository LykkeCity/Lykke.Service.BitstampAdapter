using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.BitstampAdapter.AzureRepositories;
using Lykke.Service.BitstampAdapter.AzureRepositories.Entities;
using Lykke.Service.BitstampAdapter.Services;
using Lykke.Service.BitstampAdapter.Services.Settings;
using Lykke.Service.BitstampAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.BitstampAdapter.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<AppSettings> appSettings, ILog log)
        {
            _appSettings = appSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Do not register entire settings in container, pass necessary settings to services which requires them

            var settings = _appSettings.CurrentValue.BitstampAdapterService;

            builder.RegisterInstance(settings).AsSelf();

#if (!DEBUG)
            builder.RegisterType<OrderbookPublishingService>()
                .As<IHostedService>()
                .WithParameter(new TypedParameter(typeof(OrderbookSettings), settings.Orderbooks))
                .WithParameter(new TypedParameter(typeof(RabbitMqSettings), settings.RabbitMq))
                .WithParameter(new TypedParameter(typeof(InstrumentSettings), settings.Instruments))
                .SingleInstance();
#endif

            builder.RegisterInstance(
                    new LimitOrderRepository(
                        AzureTableStorage<LimitOrder>.Create(
                            _appSettings.ConnectionString(x => x.BitstampAdapterService.Db.OrdersConnString),
                            "BitstampLimitOrders",
                            _log)))
                .SingleInstance()
                .AsSelf();
        }
    }
}
