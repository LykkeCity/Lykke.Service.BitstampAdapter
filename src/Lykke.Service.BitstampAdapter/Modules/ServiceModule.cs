using Autofac;
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

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Do not register entire settings in container, pass necessary settings to services which requires them

            var settings = _appSettings.CurrentValue.BitstampAdapterService;

            builder.RegisterType<OrderbookPublishingService>()
                .As<IHostedService>()
                .WithParameter(new TypedParameter(typeof(OrderbookSettings), settings.Orderbooks))
                .WithParameter(new TypedParameter(typeof(RabbitMqSettings), settings.RabbitMq))
                .SingleInstance();
        }
    }
}
